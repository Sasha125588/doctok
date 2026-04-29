using Api.Extensions;
using Domain.Comments;
using Domain.Shared;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Comments.Replies.List;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/comments/{commentId:long}/replies", async (
        long commentId,
        [AsParameters] CommentsRepliesListQueryParams query,
        CommentsRepository commentsRepo,
        CancellationToken ct) =>
      {
        var take = Math.Clamp(query.Limit ?? 20, 1, 50);
        var cursor = CursorCodec.Decode<CommentsCursor>(query.Cursor);
        var page = await commentsRepo.ListReplies(commentId, cursor, take + 1, ct);
        var hasNextPage = page.Count > take;
        var items = hasNextPage ? page.Take(take).ToList() : page;
        var nextCursor = hasNextPage
          ? CursorCodec.Encode(ToCursor(items[^1]))
          : null;

        return Results.Ok(new CommentsResponse(items, nextCursor));
      })
      .WithTags("Comments")
      .WithSummary("Returns replies for a comment")
      .WithName("CommentsRepliesList")
      .Produces<CommentsResponse>(StatusCodes.Status200OK)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }

  private static CommentsCursor ToCursor(CommentView item)
    => new(item.Id, item.CreatedAt);
}
