using Api.Extensions;
using Domain.Common;
using ErrorOr;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Persistence.Repos.Resolve;
using Infrastructure.Persistence.Repos.Sources;

namespace Api.Features.Resolve.Mdn;

public sealed class Handler(SourcesRepository sources, ResolveRepository resolve, JobsRepository jobs) : IHandler
{
    public async Task<ErrorOr<ResolveMdnResponse>> Handle(Query q, CancellationToken ct)
    {
        var externalRef = (q.ExternalRef ?? string.Empty).Trim().TrimStart('/');
        if (string.IsNullOrWhiteSpace(externalRef))
        {
            return Error.Validation(
                code: "Resolve.ExternalRef.Required",
                description: "externalRef is required");
        }

        var sourceId = await sources.GetSourceIdByCode(SourceCodes.Mdn, ct);

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
