using Domain.Common;
using Domain.Rules;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Persistence.Repos.Raw;
using Infrastructure.Persistence.Repos.Sources;
using Infrastructure.Persistence.Repos.Topics;

using static Domain.Common.JobTypes;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnIngestionService(
    MdnApiClient apiClient,
    MdnContentConverter converter,
    SourcesRepository sources,
    RawDocumentsRepository rawDocs,
    RawLinksRepository rawLinks,
    TopicsRepository topics,
    TopicDocumentsRepository topicDocs,
    JobsRepository jobs
)
{
    public async Task FetchRawAsync(string lang, string externalRef, CancellationToken ct)
    {
        externalRef = NormalizeExternalRef(externalRef);

        MdnApiDoc doc;
        try
        {
            doc = await apiClient.FetchAsync(lang, externalRef, ct);
        }
        catch (HttpRequestException) when (lang != "en-US")
        {
            doc = await apiClient.FetchAsync("en-US", externalRef, ct);
            lang = "en-US";
        }

        var (text, links) = converter.Convert(doc);
        var canonicalExternalRef = NormalizeExternalRef(doc.Slug);

        var sourceId = await sources.GetSourceIdByCode(SourceCodes.Mdn, ct);

        var rawId = await rawDocs.UpsertRawDocument(
            sourceId: sourceId,
            lang: lang,
            externalRef: canonicalExternalRef,
            title: doc.Title,
            content: text,
            pageType: doc.PageType,
            popularity: doc.Popularity,
            sourceModifiedAt: doc.SourceModifiedAt,
            otherLocales: doc.OtherLocales.Count > 0 ? [.. doc.OtherLocales] : null,
            ct: ct);

        var topicSlug = TextRules.TopicSlugFromExternalRef(SourceCodes.Mdn, canonicalExternalRef);
        var topicTitle = doc.Title ?? canonicalExternalRef;

        var topicId = await topics.EnsureTopic(topicSlug, topicTitle, ct);
        await topicDocs.Link(topicId, rawId, ct);

        var internalLinks = links
            .Where(x => x.Kind == "internal" && x.TargetLang is not null && x.TargetExternalRef is not null)
            .Select(x => (targetLang: LanguageHelpers.ToMdnLang(x.TargetLang!), targetExternalRef: NormalizeExternalRef(x.TargetExternalRef!), label: x.Label))
            .ToList();

        if (internalLinks.Count > 0)
        {
            await rawLinks.InsertInternalLinks(
                rawDocumentId: rawId,
                targetSourceId: sourceId,
                links: internalLinks,
                ct: ct);
        }

        var externalLinks = links
            .Where(x => x.Kind == "external" && x.Url is not null)
            .Select(x => (url: x.Url!, label: x.Label))
            .ToList();

        if (externalLinks.Count > 0)
        {
            await rawLinks.InsertExternalLinks(rawId, externalLinks, ct);
        }

        var jobKey = $"{JobTypes.GenerateFast}:{SourceCodes.Mdn}:{lang}:{canonicalExternalRef}";
        await jobs.Enqueue(
            jobType: JobTypes.GenerateFast,
            jobKey: jobKey,
            payload: new { provider = SourceCodes.Mdn, lang, externalRef = canonicalExternalRef },
            ct: ct);
    }

    private static string NormalizeExternalRef(string? externalRef)
        => (externalRef ?? string.Empty)
            .Trim()
            .TrimStart('/')
            .Replace('\\', '/')
            .ToLowerInvariant();
}
