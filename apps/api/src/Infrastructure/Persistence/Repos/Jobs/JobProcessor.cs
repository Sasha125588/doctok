namespace Infrastructure.Persistence.Repos.Jobs;

public sealed class JobProcessor
{
    public Task Process(JobEnvelope job, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}