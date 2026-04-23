using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;

namespace Api.Endpoints.Me.SavedPosts.Create;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPost("/me/saved-posts", async (
        SavePostRequest req,
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var c = new Command(userId, req.PostId);

        var result = await handler.Handle(c, ct);

        return result.ToResponse(_ => Results.NoContent());
      })
      .RequireAuthorization()
      .WithTags("Me")
      .WithSummary("Saves a post for the current user")
      .WithDescription("Creates a saved-post entry for the current user and the specified post. Repeating the request is a successful no-op.")
      .WithName("MeSavedPostsCreate")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden);
  }
}
