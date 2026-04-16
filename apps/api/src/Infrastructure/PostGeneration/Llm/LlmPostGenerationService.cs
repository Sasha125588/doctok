using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.PostGeneration.Llm;

/// <summary>
/// Generates Level-2 posts via LLM and replaces the existing Level-0 posts.
/// If the LLM is unavailable or returns an unusable response the exception
/// propagates so the job runner can retry later — Level-0 posts remain intact.
/// </summary>
public sealed class LlmPostGenerationService(
    RawDocumentsRepository rawDocsRepo,
    PostsRepository postsRepo,
    LlmPostGenerator llmPostGen,
    MarkdownHtmlRenderer mdRenderer,
    ILogger<LlmPostGenerationService> logger)
{
    public async Task EnhanceAsync(
        int sourceId,
        string sourceCode,
        string lang,
        string externalRef,
        CancellationToken ct)
    {
        var rawDocument = await rawDocsRepo.GetForPostGeneration(sourceId, lang, externalRef, ct)
                  ?? throw new InvalidOperationException(
                      $"Raw document not found: source={sourceCode}, lang={lang}, ref={externalRef}");

        var currentLevel = await postsRepo.GetMinGenerationLevel(rawDocument.Id, lang, ct);
        if (currentLevel >= 2)
        {
            logger.LogInformation(
                "Skipping LLM enhancement — already Level 2: doc={ExternalRef} lang={Lang}",
                externalRef, lang);
            return;
        }

        var llmPosts = await llmPostGen.GenerateAsync(
            rawDocument.Content,
            rawDocument.Title,
            lang,
            ct);

        if (llmPosts.Count == 0)
        {
            throw new InvalidOperationException(
                $"LLM returned no valid posts for doc={externalRef} lang={lang}");
        }

        var posts = llmPosts
            .Select(p => new PostInsert(
                Kind:            p.Kind,
                Title:           p.Title ?? rawDocument.Title,
                Body:            p.Body,
                BodyHtml:        mdRenderer.Render(p.Body),
                Position:        p.Position,
                GenerationLevel: 2))
            .ToList();

        await postsRepo.ReplaceForDocument(rawDocument.Id, rawDocument.TopicId, lang, posts, ct);

        logger.LogInformation(
            "LLM enhancement complete: {Count} posts for doc={ExternalRef} lang={Lang}",
            posts.Count, externalRef, lang);
    }
}
