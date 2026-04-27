using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.PostGeneration.Llm;

public sealed class LlmPostGenerationService(
    RawDocumentsRepository rawDocsRepo,
    PostsRepository postsRepo,
    LlmPostGenerator llmPostGen,
    MarkdownHtmlRenderer mdnRenderer,
    ILogger<LlmPostGenerationService> logger)
{
    private const string PromptVersion = "ai-rewrite-v1";

    private static readonly string[] _variantCodes = ["ai_simple", "ai_senior"];

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

        var originals = await postsRepo.GetOriginalVariantsForDocument(rawDocument.Id, lang, ct);
        if (originals.Count == 0)
        {
            logger.LogWarning(
                "No original posts to rewrite: doc={ExternalRef} lang={Lang}",
                externalRef,
                lang);
            return;
        }

        var generated = 0;
        foreach (var original in originals)
        {
            foreach (var variantCode in _variantCodes)
            {
                var rewritten = await llmPostGen.RewriteVariantAsync(
                    original.Title,
                    original.Body,
                    original.Kind,
                    variantCode,
                    lang,
                    ct);

                if (rewritten is null)
                {
                    logger.LogWarning(
                        "LLM returned no content for variant={Variant} postId={PostId}",
                        variantCode,
                        original.PostId);
                    continue;
                }

                await postsRepo.UpsertContentVariant(
                    postId: original.PostId,
                    variantCode: variantCode,
                    title: rewritten.Title,
                    body: rewritten.Body,
                    bodyHtml: mdnRenderer.Render(rewritten.Body),
                    provider: null,
                    model: null,
                    promptVersion: PromptVersion,
                    ct: ct);

                generated++;
            }
        }

        logger.LogInformation(
            "LLM variants generated: {Count} variants for {Posts} posts (doc={ExternalRef} lang={Lang})",
            generated,
            originals.Count,
            externalRef,
            lang);
    }
}
