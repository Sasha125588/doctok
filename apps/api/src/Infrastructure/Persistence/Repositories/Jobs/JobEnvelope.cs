namespace Infrastructure.Persistence.Repositories;

public sealed record JobEnvelope(
    long Id,
    string JobType,
    string JobKey,
    string PayloadJson,
    int Attempts);
