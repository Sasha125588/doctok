using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Delete;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapDelete("/comments/{commentId:long}", async (
        long commentId,
        ClaimsPrincipal user,
        CommentsRepository repo,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var deleted = await repo.Delete(commentId, userId, ct);
        if (!deleted)
        {
          return Results.NotFound();
        }

        return Results.NoContent();
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
