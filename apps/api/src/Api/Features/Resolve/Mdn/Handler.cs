using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Persistence.Repos.Resolve;
using Infrastructure.Persistence.Repos.Sources;

namespace Api.Features.Resolve.Mdn;

public sealed class Handler(SourcesRepository sources, ResolveRepository resolve, JobsRepository jobs) : IHandler
{
    public async Task<ResolveMdnResponse> Handle(Query q, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(q);

        var sourceId = await sources.GetSourceIdByCode(SourceCodes.Mdn, ct);

        var externalRef = q.ExternalRef.Trim().TrimStart('/');
        var lang = LanguageHelpers.NormalizeLang(q.Lang);

        var slug = await resolve.FindTopicSlugForDocument(sourceId, lang, externalRef, ct);
        if (slug is null)
        {
            var jobKey = $"{JobTypes.FetchRaw}:{SourceCodes.Mdn}:{lang}:{externalRef}";
            var jobId = await jobs.Enqueue(
                JobTypes.FetchRaw,
                jobKey,
                payload: new { provider = SourceCodes.Mdn, lang, externalRef },
                ct);

            return ResolveMdnResponse.Pending(jobId);
        }

        return ResolveMdnResponse.Ready(slug, lang);
    }
}
