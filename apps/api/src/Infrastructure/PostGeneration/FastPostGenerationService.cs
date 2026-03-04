using Infrastructure.Persistence.Repos.Posts;
using Infrastructure.Persistence.Repos.Raw;
using Infrastructure.Persistence.Repos.Sources;
using Infrastructure.PostGeneration.Title;

namespace Infrastructure.PostGeneration;

public sealed class FastPostGenerationService(
    RawDocumentsRepository rawDocsRepo,
    PostsRepository postsRepo,
    SourcesRepository sourcesRepo,
    FastPostGenerator postGen,
    ITitleGenerator titleGen)
{
    public async Task GenerateAsync(string sourceCode, string lang, string externalRef, CancellationToken ct)
    {
        var sourceId = await sourcesRepo.GetSourceIdByCode(sourceCode, ct);

        var row = await rawDocsRepo.GetForPostGeneration(sourceId, lang, externalRef, ct)
                  ?? throw new InvalidOperationException(
                      $"Raw document not found: source={sourceCode}, lang={lang}, ref={externalRef}");

        var rawPosts = postGen.Generate(row.Content).ToList();

        var titleTasks = rawPosts.Select(post => titleGen.GenerateTitleAsync(post.Kind, row.Title, post.Body, lang, ct));

        var titles = await Task.WhenAll(titleTasks);

        var posts = rawPosts.Zip(
          titles, (post, title) => new PostInsert(
            post.Kind,
            title ?? post.Title,
            post.Body,
            post.Position)).ToList();

        await postsRepo.ReplaceForDocument(row.Id, row.TopicId, lang, posts, ct);
    }
}
