using Infrastructure.Sources.Mdn;

namespace Infrastructure.Persistence.Repos.Jobs;

public sealed class JobProcessor(MdnIngestionService mdnIngestionService)
{
    public async Task Process(JobEnvelope job, CancellationToken ct)
    {
        switch (job.JobType)
        {
            case "fetch_raw":
            {
                var root = job.Payload.RootElement;
                var provider = root.GetProperty("provider").GetString();
                var lang = root.GetProperty("lang").GetString() ?? "en";
                var externalRef = root.GetProperty("externalRef").GetString()
                                  ?? throw new InvalidOperationException("payload.externalRef missing");

                if (provider != "mdn")
                    throw new NotSupportedException($"fetch_raw provider '{provider}' is not supported");

                await mdnIngestionService.FetchRawAsync(lang, externalRef, ct);
                return;
            }

            case "generate_fast":
                // TODO: generate cards
                return;

            default:
                throw new NotSupportedException($"Unknown job_type: {job.JobType}");
        }
    }
}