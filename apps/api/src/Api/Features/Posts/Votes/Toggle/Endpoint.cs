using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;
using Domain.Models;

namespace Api.Features.Posts.Votes.Toggle;

public sealed class Endpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
      app.MapPut("/posts/{postId:long}/vote", async (
          long postId,
          TogglePostVoteRequest req,
          ClaimsPrincipal user,
          Handler handler,
          CancellationToken ct) =>
        {
          var userId = CurrentUser.GetUserIdOrThrow(user);
          var result = await handler.Handle(new Command(postId, userId, req.Value), ct);

          return result.ToResponse(value => Results.Ok(value));
        })
        .RequireAuthorization()
        .WithTags("Votes")
        .WithSummary("Toggles a like or dislike on a post")
        .WithName("PostsVotesToggle")
        .Produces<VoteResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
