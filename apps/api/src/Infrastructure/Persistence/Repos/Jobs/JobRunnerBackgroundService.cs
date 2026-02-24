using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Repos.Jobs;

public sealed class JobRunnerBackgroundService(
    JobsRepository jobs,
    JobProcessor processor,
    ILogger<JobRunnerBackgroundService> logger
    ) : BackgroundService
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan IdleDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan LoopErrorDelay = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting JobRunnerBackgroundService");

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
                    "Processing job {JobId} type={JobType} key={JobKey} attempts={Attempts}",
                    job.Id,
                    job.JobType,
                    job.JobKey,
                    job.Attempts);

                try
                {
                    await processor.Process(job, stoppingToken);
                    await jobs.MarkDone(job.Id, stoppingToken);
                }
                catch (Exception e)
                {
                    if (job.Attempts >= MaxAttempts)
                    {
                        logger.LogError(e, "Job {JobId} permanently failed after {Attempts} attempts", job.Id, job.Attempts);
                        await jobs.MarkFailed(job.Id, e.Message, stoppingToken);
                    }
                    else
                    {
                        logger.LogWarning(e, "Job {JobId} failed (attempt {Attempts}/{MaxAttempts}), will retry", job.Id, job.Attempts, MaxAttempts);
                        await jobs.MarkPendingForRetry(job.Id, e.Message, stoppingToken);

                        var retryDelay = CalculateRetryDelay(job.Attempts);
                        await Task.Delay(retryDelay, stoppingToken);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Job runner loop error");
                await Task.Delay(LoopErrorDelay, stoppingToken);
            }
        }

        logger.LogInformation("Job runner stopped");
    }

    private static TimeSpan CalculateRetryDelay(int attempts)
    {
        var exponent = Math.Clamp(attempts - 1, 0, 4);
        var seconds = Math.Min(30d, Math.Pow(2d, exponent));
        var jitterMs = Random.Shared.Next(0, 300);

        return TimeSpan.FromSeconds(seconds) + TimeSpan.FromMilliseconds(jitterMs);
    }
}
