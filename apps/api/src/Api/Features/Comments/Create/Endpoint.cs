using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Create;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPost("/posts/{postId:long}/comments", async (
        long postId,
        CreateCommentRequest req,
        ClaimsPrincipal user,
        CommentsRepository commentsRepo,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var comment = await commentsRepo.CreateRoot(postId, userId, req.Body.Trim(), ct);
        return Results.Created($"/api/posts/{postId}/comments/{comment.Id}", comment);
      })
      .RequireAuthorization()
      .WithTags("Comments")
      .WithSummary("Adds a comment to a post")
      .WithName("PostsCommentsCreate")
      .Produces<Domain.Models.Comment>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized);
  }
}
