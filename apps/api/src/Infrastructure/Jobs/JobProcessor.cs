using System.Text.Json;
using Domain.Common;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Sources.Common;

namespace Infrastructure.Jobs;

public sealed class JobProcessor(IEnumerable<ISourceJobHandler> sourceHandlers)
{
    private readonly Dictionary<string, ISourceJobHandler> _sourceHandlers = sourceHandlers
        .GroupBy(x => x.SourceCode, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(x => x.Key, x => x.Single(), StringComparer.OrdinalIgnoreCase);

    public async Task Process(JobEnvelope job, CancellationToken ct)
    {
        using var payload = JsonDocument.Parse(job.PayloadJson);
        var root = payload.RootElement;

        var provider = root.GetProperty("provider").GetString()
                       ?? throw new InvalidOperationException("payload.provider missing");
        var lang = root.GetProperty("lang").GetString() ?? "en";
        var externalRef = root.GetProperty("externalRef").GetString()
                          ?? throw new InvalidOperationException("payload.externalRef missing");

        if (!_sourceHandlers.TryGetValue(provider, out var sourceHandler))
        {
            throw new NotSupportedException($"provider '{provider}' is not supported");
        }

        switch (job.JobType)
        {
            case JobTypes.FetchRaw:
                await sourceHandler.FetchRawAsync(lang, externalRef, ct);
                return;

            case JobTypes.GenerateFast:
                await sourceHandler.GenerateFastPostsAsync(lang, externalRef, ct);
                return;

            default:
                throw new NotSupportedException($"Unknown job_type: {job.JobType}");
        }
    }
}
