using System.Security.Claims;
using Api.Auth;
using Api.Extensions;

namespace Api.Endpoints.Me.SavedPosts.List;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/me/saved-posts", async (
        [AsParameters] SavedPostsListQueryParams query,
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var c = new Query(query.Cursor, userId, query.Limit);

        var result = await handler.Handle(c, ct);

        return Results.Ok(result);
      })
      .RequireAuthorization()
      .WithTags("Me")
      .WithSummary("Returns paginated saved posts for the current user")
      .WithDescription("Returns the current user's saved posts ordered by save time descending.")
      .WithName("MeSavedPostsList")
      .Produces<SavedPostsResponse>(StatusCodes.Status200OK)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden);
  }
}
