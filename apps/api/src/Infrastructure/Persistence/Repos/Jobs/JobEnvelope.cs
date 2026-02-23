namespace Infrastructure.Persistence.Repos.Jobs;

public sealed record JobEnvelope(
    long Id,
    string JobType,
    string JobKey,
    string PayloadJson,
    int Attempts);
