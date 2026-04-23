using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;

namespace Api.Endpoints.Me.SavedPosts.Delete;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapDelete("/me/saved-posts/{postId:long}", async (
        long postId,
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var c = new Command(userId, postId);

        var result = await handler.Handle(c, ct);

        return result.ToResponse(_ => Results.NoContent());
      })
      .RequireAuthorization()
      .WithTags("Me")
      .WithSummary("Removes a saved post for the current user")
      .WithDescription("Deletes the current user's saved-post entry for the specified post. Repeating the request is a successful no-op.")
      .WithName("MeSavedPostsDelete")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden);
  }
}
