using Api.Extensions;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Posts.Comments.List;

public sealed class ListCommentsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/posts/{postId:long}/comments", async (
            long postId,
            int? limit,
            CommentsRepository repo,
            CancellationToken ct) =>
        {
            var take = Math.Clamp(limit ?? 20, 1, 50);

            var items = await repo.ListRoots(postId, take, ct);

            return Results.Ok(items);
        })
        .WithTags("Comments")
        .WithSummary("Returns root comments for a post")
        .Produces<IReadOnlyList<CommentDto>>(StatusCodes.Status200OK);
    }
}
