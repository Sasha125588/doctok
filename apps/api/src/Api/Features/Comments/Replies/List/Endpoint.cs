using Api.Extensions;
using Api.Features.Comments.Shared;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Replies.List;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/comments/{commentId:long}/replies", async (
        long commentId,
        int? limit,
        CommentsRepository commentsRepo,
        CancellationToken ct) =>
      {
        var take = Math.Clamp(limit ?? 20, 1, 50);
        var items = (await commentsRepo.ListReplies(commentId, take, ct)).ToResponses();
        return Results.Ok(items);
      })
      .WithTags("Comments")
      .WithSummary("Returns replies for a root comment")
      .WithName("CommentsRepliesList")
      .Produces<IReadOnlyList<CommentResponse>>(StatusCodes.Status200OK);
  }
}
