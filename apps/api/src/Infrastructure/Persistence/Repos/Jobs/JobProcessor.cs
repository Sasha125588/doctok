using System.Text.Json;
using Domain.Common;
using Infrastructure.Cards;
using Infrastructure.Sources.Mdn;

namespace Infrastructure.Persistence.Repos.Jobs;

public sealed class JobProcessor(MdnIngestionService mdnIngestionService, FastCardGenerationService fastCardGenerationService)
{
    public async Task Process(JobEnvelope job, CancellationToken ct)
    {
        using var payload = JsonDocument.Parse(job.PayloadJson);
        var root = payload.RootElement;

        switch (job.JobType)
        {
            case JobTypes.FetchRaw:
            {
                var provider = root.GetProperty("provider").GetString();
                var lang = root.GetProperty("lang").GetString() ?? "en";
                var externalRef = root.GetProperty("externalRef").GetString()
                                  ?? throw new InvalidOperationException("payload.externalRef missing");

                if (provider != SourceCodes.Mdn)
                    throw new NotSupportedException($"fetch_raw provider '{provider}' is not supported");

                await mdnIngestionService.FetchRawAsync(lang, externalRef, ct);
                return;
            }

            case JobTypes.GenerateFast:
            {
                var provider = root.GetProperty("provider").GetString();
                var lang = root.GetProperty("lang").GetString()!;
                var externalRef = root.GetProperty("externalRef").GetString()!;

                if (provider != SourceCodes.Mdn)
                    throw new NotSupportedException();

                await fastCardGenerationService.GenerateAsync(lang, externalRef, ct);
                return;
            }

            default:
                throw new NotSupportedException($"Unknown job_type: {job.JobType}");
        }
    }
}
