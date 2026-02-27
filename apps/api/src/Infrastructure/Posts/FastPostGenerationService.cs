using Infrastructure.Persistence.Repos.Posts;
using Infrastructure.Persistence.Repos.Raw;
using Infrastructure.Persistence.Repos.Sources;

namespace Infrastructure.Posts;

public sealed class FastPostGenerationService(
    RawDocumentsRepository rawDocs,
    PostsRepository postsRepo,
    SourcesRepository sources,
    FastPostGenerator gen)
{
    public async Task GenerateAsync(string sourceCode, string lang, string externalRef, CancellationToken ct)
    {
        var sourceId = await sources.GetSourceIdByCode(sourceCode, ct);

        var row = await rawDocs.GetForPostGeneration(sourceId, lang, externalRef, ct)
                  ?? throw new InvalidOperationException(
                      $"Raw document not found: source={sourceCode}, lang={lang}, ref={externalRef}");

        var posts = gen.Generate(row.Content)
            .Select(post => new PostInsert(post.Kind, post.Title, post.Body, post.Position));

        await postsRepo.ReplaceForDocument(row.Id, row.Topic_Id, lang, posts, ct);
    }
}
