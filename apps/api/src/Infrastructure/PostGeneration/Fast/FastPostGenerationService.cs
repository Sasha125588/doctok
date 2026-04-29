using System.Text.Json;
using Domain.Jobs;
using Domain.Mdn;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Sources.Mdn;
using Microsoft.Extensions.Logging;

namespace Infrastructure.PostGeneration.Fast;

public sealed class FastPostGenerationService(
    RawDocumentsRepository rawDocsRepo,
    PostsRepository postsRepo,
    FastPostGenerator postGen,
    MarkdownHtmlRenderer mdnRenderer,
    MdnMarkdownConverter mdnMarkdownConverter,
    JobsRepository jobs,
    ILogger<FastPostGenerationService> logger)
{
    public async Task GenerateAsync(
        int sourceId,
        string sourceCode,
        string lang,
        string externalRef,
        CancellationToken ct)
    {
        var rawDocument = await rawDocsRepo.GetForPostGeneration(sourceId, lang, externalRef, ct)
                  ?? throw new InvalidOperationException(
                      $"Raw document not found: source={sourceCode}, lang={lang}, ref={externalRef}");

        var rawSections = JsonSerializer.Deserialize<IReadOnlyList<MdnSection>>(rawDocument.SectionsJson)
                          ?? [];

        if (rawSections.Count == 0)
        {
            if (!await postsRepo.ActivePostsExistForDocument(rawDocument.Id, lang, ct))
            {
                throw new InvalidOperationException(
                    $"Cannot generate fast posts because raw document has no sections: source={sourceCode}, lang={lang}, ref={externalRef}");
            }

            logger.LogWarning(
                "Keeping existing active posts because raw document has no sections: rawDocumentId={RawDocumentId} lang={Lang} externalRef={ExternalRef}",
                rawDocument.Id,
                lang,
                externalRef);
            return;
        }

        var markdownSections = rawSections
            .Select(s => s with { Content = mdnMarkdownConverter.ConvertHtml(s.Content).Markdown })
            .ToList();

        var rawPosts = postGen.Generate(markdownSections);

        var posts = rawPosts
            .Select(p => new OriginalPostInsert(
                SourceSectionKey: p.SourceSectionKey,
                Kind:             p.Kind,
                Title:            p.Title ?? rawDocument.Title,
                Body:             p.Body,
                BodyHtml:         mdnRenderer.Render(p.Body),
                Position:         p.Position))
            .ToList();

        if (posts.Count == 0)
        {
            if (!await postsRepo.ActivePostsExistForDocument(rawDocument.Id, lang, ct))
            {
                throw new InvalidOperationException(
                    $"Cannot generate fast posts because all sections produced empty/skipped posts: source={sourceCode}, lang={lang}, ref={externalRef}");
            }

            logger.LogWarning(
                "Keeping existing active posts because no posts were generated: rawDocumentId={RawDocumentId} lang={Lang} externalRef={ExternalRef} sections={SectionCount}",
                rawDocument.Id,
                lang,
                externalRef,
                rawSections.Count);
            return;
        }

        await postsRepo.UpsertOriginalVariantsForDocument(rawDocument.Id, rawDocument.TopicId, lang, posts, ct);

        var jobKey = $"{JobTypes.GenerateLlm}:{sourceCode}:{lang}:{externalRef}";
        await jobs.Enqueue(
            jobType: JobTypes.GenerateLlm,
            jobKey:  jobKey,
            payload: new { provider = sourceCode, lang, externalRef },
            ct:      ct);
    }
}
