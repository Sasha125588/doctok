using System.Text.Json;

namespace Infrastructure.Persistence.Repos.Jobs;

public sealed record JobEnvelope(
    long Id,
    string JobType,
    string JobKey,
    JsonDocument Payload,
    int Attempts);