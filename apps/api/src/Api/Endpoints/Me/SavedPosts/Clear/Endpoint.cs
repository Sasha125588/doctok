using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;

namespace Api.Endpoints.Me.SavedPosts.Clear;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapDelete("/me/saved-posts", async (
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var c = new Command(userId);

        var result = await handler.Handle(c, ct);

        return result.ToResponse(affected => Results.Ok(new ClearSavedPostsResponse(affected)));
      })
      .RequireAuthorization()
      .WithTags("Me")
      .WithSummary("Clears saved posts for the current user")
      .WithDescription("Deletes all saved-post entries for the current user and returns the number of deleted posts.")
      .WithName("MeSavedPostsClear")
      .Produces<ClearSavedPostsResponse>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden);
  }
}
