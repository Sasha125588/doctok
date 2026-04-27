using Domain.Jobs;
using Domain.Mdn;
using Domain.Shared;
using Domain.Sources;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnIngestionService(
    MdnApiClient apiClient,
    MdnContentConverter converter,
    SourcesRepository sources,
    RawDocumentsRepository rawDocs,
    RawLinksRepository rawLinks,
    TopicsRepository topics,
    TopicDocumentsRepository topicDocs,
    JobsRepository jobs,
    ILogger<MdnIngestionService> logger
)
{
    public async Task FetchRawAsync(string lang, string externalRef, CancellationToken ct)
    {
        lang = LanguageHelpers.NormalizeLang(lang);
        externalRef = ExternalRefHelpers.Normalize(externalRef);

        logger.LogInformation(
            "Ingesting MDN doc lang={Lang} ref={ExternalRef}", lang, externalRef);

        MdnDocument doc = await apiClient.FetchAsync(lang, externalRef, ct);
        var (text, links) = converter.Convert(doc);

        var canonicalExternalRef = ExternalRefHelpers.Normalize(doc.Slug);

        var sourceId = await sources.GetSourceIdByCode(SourceCodes.Mdn, ct);

        var rawId = await rawDocs.UpsertRawDocument(
            sourceId: sourceId,
            lang: lang,
            externalRef: canonicalExternalRef,
            title: doc.Title,
            content: text,
            pageType: doc.PageType,
            popularity: doc.Popularity ?? 0,
            sourceModifiedAt: doc.SourceModifiedAt,
            otherLocales: doc.OtherLocales.Count > 0 ? [.. doc.OtherLocales] : null,
            ct: ct);

        var topicSlug = SourceCodes.Mdn + "/" + canonicalExternalRef;
        var topicTitle = doc.Title;

        var topicId = await topics.EnsureTopic(topicSlug, topicTitle, ct);
        await topicDocs.Link(topicId, rawId, ct);

        var internalLinks = links
            .Where(x => x is { Kind: "internal", TargetLang: not null, TargetExternalRef: not null })
            .Select(x => (
                targetLang: LanguageHelpers.NormalizeLang(x.TargetLang!),
                targetExternalRef: ExternalRefHelpers.Normalize(x.TargetExternalRef!),
                label: x.Label))
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
            .Where(x => x is { Kind: "external", Url: not null })
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

        logger.LogInformation(
            "Ingestion complete: rawId={RawId} topicSlug={TopicSlug} internalLinks={InternalLinks} externalLinks={ExternalLinks}",
            rawId,
            topicSlug,
            internalLinks.Count,
            externalLinks.Count);
    }
}
