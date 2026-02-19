using Domain.Rules;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Persistence.Repos.Raw;
using Infrastructure.Persistence.Repos.Sources;
using Infrastructure.Persistence.Repos.Topics;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnIngestionService(
    MdnIndex index,
    MdnRawParser parser,
    MdnLinkExtractor linkExtractor,
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
        lang = NormalizeLang(lang);
        externalRef = NormalizeExternalRef(externalRef);

        await index.BuildOnceAsync(ct);

        var path = index.TryGetPath(lang, externalRef);
        if (path is null)
        {
            if (lang != "en")
                path = index.TryGetPath("en", externalRef);

            if (path is null)
                throw new FileNotFoundException($"MDN doc not found (lang={lang}, externalRef={externalRef})");
        }

        var parsed = await parser.ParseAsync(path, ct);
        var canonicalExternalRef = NormalizeExternalRef(parsed.ExternalRef);

        var sourceId = await sources.GetSourceIdByCode("mdn", ct);

        var rawId = await rawDocs.UpsertRawDocument(
            sourceId: sourceId,
            lang: lang,
            externalRef: canonicalExternalRef,
            title: parsed.Title,
            content: parsed.Content,
            ct: ct);

        var topicSlug = TextRules.TopicSlugFromExternalRef("mdn", canonicalExternalRef);
        var topicTitle = parsed.Title ?? canonicalExternalRef;

        var topicId = await topics.EnsureTopic(topicSlug, topicTitle, ct);
        await topicDocs.Link(topicId, rawId, ct);

        var links = linkExtractor.Extract(parsed.Content);

        var internalLinks = links
            .Where(x => x.Kind == "internal" && x.TargetLang is not null && x.TargetExternalRef is not null)
            .Select(x => (targetLang: NormalizeLang(x.TargetLang!), targetExternalRef: NormalizeExternalRef(x.TargetExternalRef!), label: x.Label))
            .ToList();

        if (internalLinks.Count > 0)
        {
            await rawLinks.InsertInternalLinks(
                rawDocumentId: rawId,
                targetSourceId: sourceId, // MDN links -> MDN
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

        var jobKey = $"generate_fast:mdn:{lang}:{canonicalExternalRef}";
        await jobs.Enqueue(
            jobType: "generate_fast",
            jobKey: jobKey,
            payload: new { provider = "mdn", lang, externalRef = canonicalExternalRef },
            ct: ct);
    }

    private static string NormalizeLang(string? lang)
        => (lang ?? "en").Trim().ToLowerInvariant() switch
        {
            "en" or "en-us" => "en",
            "ru" => "ru",
            _ => (lang ?? "en").Trim().ToLowerInvariant()
        };

    private static string NormalizeExternalRef(string externalRef)
        => (externalRef ?? "")
            .Trim()
            .TrimStart('/')
            .Replace('\\', '/');
}
