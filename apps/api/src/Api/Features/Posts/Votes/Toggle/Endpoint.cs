using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Domain.Models;
using Infrastructure.Persistence.Repos.Votes;

namespace Api.Features.Posts.Votes.Toggle;

public sealed class Endpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
      app.MapPut("/posts/{postId:long}/vote", async (
          long postId,
          TogglePostVoteRequest req,
          ClaimsPrincipal user,
          VotesRepository repo,
          CancellationToken ct) =>
        {
          var userId = CurrentUser.GetUserIdOrThrow(user);
          var result = await repo.Toggle(postId, userId, req.Value, ct);
          return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Votes")
        .WithSummary("Toggles a like or dislike on a post")
        .WithName("PostsVotesToggle")
        .Produces<VoteResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
