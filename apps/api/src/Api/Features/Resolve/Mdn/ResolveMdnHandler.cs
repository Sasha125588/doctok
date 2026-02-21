using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Persistence.Repos.Resolve;
using Infrastructure.Persistence.Repos.Sources;

namespace Api.Features.Resolve.Mdn;

public sealed class ResolveMdnHandler(SourcesRepository sources, ResolveRepository resolve, JobsRepository jobs)
{
    public async Task<ResolveMdnResult> Handle(ResolveMdnQuery q, CancellationToken ct)
    {
    ArgumentNullException.ThrowIfNull(q);

    var sourceId = await sources.GetSourceIdByCode("mdn", ct);

    var externalRef = q.ExternalRef.Trim().TrimStart('/');
    var lang = (q.Lang ?? "en").Trim().ToLowerInvariant();

    var slug = await resolve.FindTopicSlugForDocument(sourceId, lang, externalRef, ct);
    if (slug is null)
    {
        var jobKey = $"fetch_raw:mdn:{lang}:{externalRef}";
        var jobId = await jobs.Enqueue(
            "fetch_raw",
            jobKey,
            payload: new { provider = "mdn", lang, externalRef },
            ct);

        return ResolveMdnResult.Pending(jobId);
    }

    return ResolveMdnResult.Ready(slug, lang);
    }
}
