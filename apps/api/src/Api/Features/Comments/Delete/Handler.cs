using Api.Extensions;
using ErrorOr;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Delete;

public sealed class Handler(CommentsRepository commentsRepo) : IHandler
{
  public async Task<ErrorOr<bool>> Handle(Command command, CancellationToken ct)
  {
    var deleted = await commentsRepo.Delete(command.CommentId, command.UserId, ct);
    if (!deleted)
    {
      return Error.NotFound(
        code: "Comments.NotFound",
        description: $"Comment '{command.CommentId}' was not found.");
    }

    return true;
  }
}
