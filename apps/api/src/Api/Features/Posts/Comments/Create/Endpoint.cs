using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Api.Features.Comments.Shared;

namespace Api.Features.Posts.Comments.Create;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPost("/posts/{postId:long}/comments", async (
        long postId,
        CreatePostCommentRequest req,
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);

        var response = await handler.Handle(new Command(postId, userId, req.Body), ct);
        return Results.Created($"/api/posts/{postId}/comments/{response.Id}", response);
      })
      .RequireAuthorization()
      .WithTags("Comments")
      .WithSummary("Adds a comment to a post")
      .WithName("PostsCommentsCreate")
      .Produces<CommentResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized);
  }
}
