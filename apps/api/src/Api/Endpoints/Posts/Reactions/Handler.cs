using Api.Extensions;
using Domain.Reactions;
using ErrorOr;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Posts.Reactions;

public sealed class Handler(PostReactionsRepository postReactionsRepo) : IHandler
{
  public async Task<ErrorOr<ReactionView>> Handle(Command command, CancellationToken ct)
  {
    var reactionResult = await postReactionsRepo.Toggle(
      command.PostId,
      command.UserId,
      command.Value.ToString().ToLowerInvariant(),
      ct);

    if (reactionResult is null)
    {
      return Error.NotFound(
        code: "Reactions.PostNotFound",
        description: $"Post '{command.PostId}' was not found.");
    }

    return reactionResult;
  }
}
