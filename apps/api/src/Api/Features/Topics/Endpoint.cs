using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics;

public sealed class TopicsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/{slug}", async (
            string slug,
            string? lang,
            TopicReadRepository topicRepo,
            CancellationToken ct) =>
        {
            var resolvedLang = LanguageHelpers.NormalizeLang(lang ?? "en");
            var posts = await topicRepo.GetPosts(slug, resolvedLang, ct);

            if (posts.Count == 0)
            {
                return Results.NotFound();
            }

            return Results.Ok(posts.Select(post => new Response(
                post.Id,
                post.Kind,
                post.Body,
                post.Position,
                post.Like_Count,
                post.Dislike_Count,
                post.Comment_Count)));
        });
    }
}
