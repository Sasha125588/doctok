using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;
using Domain.Comments;
using Domain.Reactions;

namespace Api.Endpoints.Posts.Reactions;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPatch("/posts/{postId:long}/reactions", async (
        long postId,
        TogglePostReactionRequest req,
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);
        var result = await handler.Handle(new Command(postId, userId, req.Value), ct);

        return result.ToResponse(Results.Ok);
      })
      .RequireAuthorization()
      .WithTags("Reactions")
      .WithSummary("Toggles a like or dislike on a post")
      .WithDescription(
        "Sets the current user's reaction for a post. Allowed values are 'like' and 'dislike'. " +
        "Sending the same value again removes the reaction.")
      .WithName("PostsReactionsToggle")
      .Produces<ReactionView>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status401Unauthorized)
      .ProducesProblem(StatusCodes.Status403Forbidden)
      .ProducesProblem(StatusCodes.Status404NotFound);
  }
}
