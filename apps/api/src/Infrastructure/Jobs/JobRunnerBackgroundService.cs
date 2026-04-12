using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public sealed class JobRunnerBackgroundService(
    JobsRepository jobs,
    JobProcessor processor,
    ILogger<JobRunnerBackgroundService> logger
    ) : BackgroundService
{
    private const int WorkersCount = 5;
    private const int MaxAttempts = 3;
    private static readonly TimeSpan StaleRunningThreshold = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan IdleDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan LoopErrorDelay = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting JobRunnerBackgroundService");

        var recovered = await jobs.RequeueStaleRunning(StaleRunningThreshold, stoppingToken);
        if (recovered > 0)
        {
            logger.LogWarning("Recovered {RecoveredJobs} stale running jobs", recovered);
        }

        var tasks = Enumerable.Range(0, WorkersCount)
          .Select(i => RunWorker(i, stoppingToken));

        await Task.WhenAll(tasks);

        logger.LogInformation("Job runner stopped");
    }

    private async Task RunWorker(int workerId, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await jobs.Dequeue(stoppingToken);

                if (job is null)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                    continue;
                }

                logger.LogInformation(
                    "Processing workerId {WorkerId} job {JobId} type={JobType} key={JobKey} attempts={Attempts}",
                    workerId,
                    job.Id,
                    job.JobType,
                    job.JobKey,
                    job.Attempts);

                try
                {
                    await processor.Process(job, stoppingToken);
                    await jobs.MarkDone(job.Id, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                  throw;
                }
                catch (Exception e)
                {
                  var error = $"{e.GetType().Name}: {e.Message}";

                  if (job.Attempts >= MaxAttempts)
                  {
                    logger.LogError(e, "Job {JobId} permanently failed after {Attempts} attempts", job.Id, job.Attempts);
                    await jobs.MarkFailed(job.Id, error, stoppingToken);
                  }
                  else
                  {
                    logger.LogWarning(
                      e,
                      "Job {JobId} failed (attempt {Attempts}/{MaxAttempts}), will retry",
                      job.Id,
                      job.Attempts,
                      MaxAttempts);

                    var retryDelay = CalculateRetryDelay(job.Attempts);
                    await jobs.MarkPendingForRetry(job.Id, error, retryDelay, stoppingToken);
                  }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
              logger.LogError(e, "Worker {WorkerId} loop error", workerId);
              await Task.Delay(LoopErrorDelay, stoppingToken);
            }
        }
    }

    private static TimeSpan CalculateRetryDelay(int attempts)
    {
        var exponent = Math.Clamp(attempts - 1, 0, 4);
        var seconds = Math.Min(30d, Math.Pow(2d, exponent));
        var jitterMs = Random.Shared.Next(0, 300);

        return TimeSpan.FromSeconds(seconds) + TimeSpan.FromMilliseconds(jitterMs);
    }
}
