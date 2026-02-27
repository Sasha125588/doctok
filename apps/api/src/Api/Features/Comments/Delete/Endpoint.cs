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
        await repo.Delete(commentId, userId, ct);
        return Results.NoContent();
      })
      .RequireAuthorization()
      .WithTags("Comments");
  }
}
