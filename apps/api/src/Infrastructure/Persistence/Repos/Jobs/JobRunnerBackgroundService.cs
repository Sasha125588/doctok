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
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
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
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Job runner loop error");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        logger.LogInformation("Job runner stopped");
    }
}
