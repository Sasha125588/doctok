using Domain.Common;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.PostGeneration.Fast;

/// <summary>
/// Generates Level-0 posts (section-based, no LLM) and immediately enqueues
/// a <see cref="JobTypes.GenerateLlm"/> job so the LLM can later replace them
/// with higher-quality Level-2 posts.
/// </summary>
public sealed class FastPostGenerationService(
    RawDocumentsRepository rawDocsRepo,
    PostsRepository postsRepo,
    FastPostGenerator postGen,
    MarkdownHtmlRenderer mdRenderer,
    JobsRepository jobs)
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

        var rawPosts = postGen.Generate(rawDocument.Content);

        var posts = rawPosts
            .Select(p => new PostInsert(
                Kind:            p.Kind,
                Title:           p.Title ?? rawDocument.Title,
                Body:            p.Body,
                BodyHtml:        mdRenderer.Render(p.Body),
                Position:        p.Position,
                GenerationLevel: 0))
            .ToList();

        await postsRepo.ReplaceForDocument(rawDocument.Id, rawDocument.TopicId, lang, posts, ct);

        // Enqueue LLM enhancement — will replace these posts once the LLM is available.
        var jobKey = $"{JobTypes.GenerateLlm}:{sourceCode}:{lang}:{externalRef}";
        await jobs.Enqueue(
            jobType: JobTypes.GenerateLlm,
            jobKey:  jobKey,
            payload: new { provider = sourceCode, lang, externalRef },
            ct:      ct);
    }
}
