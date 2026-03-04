using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;

namespace Api.Features.Comments.Delete;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapDelete("/comments/{commentId:long}", async (
        long commentId,
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var result = await handler.Handle(new Command(commentId, userId), ct);

        return result.ToResponse(_ => Results.NoContent());
      })
      .RequireAuthorization()
      .WithTags("Comments")
      .WithSummary("Soft-deletes a comment owned by the current user")
      .WithName("CommentsDelete")
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status404NotFound);
  }
}
