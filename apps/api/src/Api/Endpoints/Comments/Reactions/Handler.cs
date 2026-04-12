using Api.Extensions;
using Domain.Common;
using Domain.Models;
using ErrorOr;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Comments.Reactions;

public sealed class Handler(CommentReactionsRepository commentReactionsRepo) : IHandler
{
  public async Task<ErrorOr<ReactionResult>> Handle(Command command, CancellationToken ct)
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
