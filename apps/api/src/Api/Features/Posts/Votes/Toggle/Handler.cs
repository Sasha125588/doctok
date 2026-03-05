using Api.Extensions;
using Domain.Common;
using Domain.Models;
using ErrorOr;
using Infrastructure.Persistence.Repos.Votes;

namespace Api.Features.Posts.Votes.Toggle;

public sealed class Handler(VotesRepository votesRepo) : IHandler
{
  public async Task<ErrorOr<VoteResult>> Handle(Command command, CancellationToken ct)
  {
    var voteResult = await votesRepo.Toggle(
      command.PostId,
      command.UserId,
      command.Value.ToStorageValue(),
      ct);

    if (voteResult is null)
    {
      return Error.NotFound(
        code: "Votes.PostNotFound",
        description: $"Post '{command.PostId}' was not found.");
    }

    return voteResult;
  }
}
