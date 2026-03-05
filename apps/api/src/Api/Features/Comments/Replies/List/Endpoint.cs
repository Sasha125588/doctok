using Api.Extensions;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Replies.List;

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
        var items = await commentsRepo.ListReplies(commentId, take, ct);
        return Results.Ok(items);
      })
      .WithTags("Comments")
      .WithSummary("Returns replies for a root comment")
      .WithName("CommentsRepliesList")
      .Produces<IReadOnlyList<Domain.Models.Comment>>(StatusCodes.Status200OK)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }
}
