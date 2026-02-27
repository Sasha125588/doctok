using Api.Extensions;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Replies.List;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/comments/{commentId:long}/replies", async (
        long commentId,
        int? limit,
        CommentsRepository repo,
        CancellationToken ct) =>
      {
        var take = Math.Clamp(limit ?? 20, 1, 50);
        var items = await repo.ListReplies(commentId, take, ct);
        return Results.Ok(items);
      })
      .WithTags("Comments");
  }
}
