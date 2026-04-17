using Api.Extensions;
using Domain.Reactions;
using ErrorOr;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Comments.Reactions;

public sealed class Handler(CommentReactionsRepository commentReactionsRepo) : IHandler
{
  public async Task<ErrorOr<ReactionView>> Handle(Command command, CancellationToken ct)
  {
    var reactionResult = await commentReactionsRepo.Toggle(
      command.CommentId,
      command.UserId,
      command.Value.ToStorageValue(),
      ct);

    if(reactionResult is null)
    {
      return Error.NotFound(
        code: "Reactions.CommentNotFound",
        description: $"Comment '{command.CommentId}' was not found.");
    }

    return reactionResult;
  }
}
