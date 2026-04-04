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

        var rawDocument = await rawDocsRepo.GetForPostGeneration(sourceId, lang, externalRef, ct)
                  ?? throw new InvalidOperationException(
                      $"Raw document not found: source={sourceCode}, lang={lang}, ref={externalRef}");

        var rawPosts = postGen.Generate(rawDocument.Content).ToList();

        var titleTasks = rawPosts.Select(post => titleGen.GenerateTitleAsync(post.Kind, rawDocument.Title, post.Body, lang, ct));

        var titles = await Task.WhenAll(titleTasks);

        var posts = rawPosts.Zip(
          titles, (post, title) => new PostInsert(
            post.Kind,
            title ?? post.Title ?? rawDocument.Title,
            post.Body,
            post.Position)).ToList();

        await postsRepo.ReplaceForDocument(rawDocument.Id, rawDocument.TopicId, lang, posts, ct);
    }
}
