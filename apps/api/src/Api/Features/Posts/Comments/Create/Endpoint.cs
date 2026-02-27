using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Posts.Comments.Create;

public sealed class CreateCommentEndpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPost("/posts/{postId:long}/comments", async (
        long postId,
        Request req,
        ClaimsPrincipal user,
        CreateCommentHandler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);

        var dto = await handler.Handle(new Command(postId, userId, req.Body), ct);

        return Results.Ok(dto);
      })
      .RequireAuthorization()
      .WithTags("Comments")
      .WithSummary("Adds a comment to a post")
      .Produces<CommentDto>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized);
  }
}
