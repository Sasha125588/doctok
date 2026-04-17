using Api.Extensions;
using Domain.Comments;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Comments.List;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/posts/{postId:long}/comments", async (
        long postId,
        [AsParameters] PostsCommentsListQueryParams query,
        CommentsRepository commentsRepo,
        CancellationToken ct) =>
      {
        var take = Math.Clamp(query.Limit ?? 20, 1, 50);
        var items = await commentsRepo.ListRoots(postId, take, ct);
        return Results.Ok(items);
      })
      .WithTags("Comments")
      .WithSummary("Returns root comments for a post")
      .WithName("PostsCommentsList")
      .Produces<IReadOnlyList<Comment>>(StatusCodes.Status200OK)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }
}
