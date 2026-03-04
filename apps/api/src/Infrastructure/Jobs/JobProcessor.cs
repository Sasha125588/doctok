using System.Text.Json;
using Domain.Common;
using Infrastructure.Persistence.Repos.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Jobs;

public sealed class JobProcessor(IServiceProvider serviceProvider)
{
    public async Task Process(JobEnvelope job, CancellationToken ct)
    {
        using var payload = JsonDocument.Parse(job.PayloadJson);
        var root = payload.RootElement;

        var provider = root.GetProperty("provider").GetString()
                       ?? throw new InvalidOperationException("payload.provider missing");
        var lang = root.GetProperty("lang").GetString() ?? "en";
        var externalRef = root.GetProperty("externalRef").GetString()
                          ?? throw new InvalidOperationException("payload.externalRef missing");

        var sourceHandler = serviceProvider.GetKeyedService<ISourceJobHandler>(provider)
                            ?? throw new NotSupportedException($"provider '{provider}' is not supported");

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
