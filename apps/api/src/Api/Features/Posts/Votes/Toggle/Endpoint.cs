using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Infrastructure.Persistence.Repos.Votes;

namespace Api.Features.Posts.Votes.Toggle;

public sealed class ToggleVoteEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
      app.MapPut("/posts/{postId:long}/vote", async (
          long postId,
          Request req,
          ClaimsPrincipal user,
          VotesRepository repo,
          CancellationToken ct) =>
        {
          var userId = CurrentUser.GetUserIdOrThrow(user);
          var res = await repo.Toggle(postId, userId, req.Value, ct);
          return Results.Ok(res);
        })
        .RequireAuthorization()
        .WithTags("Votes")
        .WithSummary("Toggles a like or dislike on a post")
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
