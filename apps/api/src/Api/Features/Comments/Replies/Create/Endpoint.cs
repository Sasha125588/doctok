using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;

namespace Api.Features.Comments.Replies.Create;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPost("/comments/{commentId:long}/replies", async (
        long commentId,
        CreateCommentRequest req,
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var result = await handler.Handle(new Command(commentId, userId, req.Body.Trim()), ct);

        return result.ToResponse(comment => Results.Created($"/api/comments/{commentId}/replies/{comment.Id}", comment));
      })
      .RequireAuthorization()
      .WithTags("Comments")
      .WithSummary("Adds a reply to a root comment")
      .WithName("CommentsRepliesCreate")
      .Produces<Domain.Models.Comment>(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status404NotFound);
  }
}
