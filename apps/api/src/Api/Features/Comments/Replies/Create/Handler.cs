using Api.Extensions;
using ErrorOr;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Replies.Create;

public sealed class Handler(CommentsRepository commentsRepo) : IHandler
{
  public async Task<ErrorOr<Domain.Models.Comment>> Handle(Command command, CancellationToken ct)
  {
    try
    {
      var comment = await commentsRepo.Reply(command.ParentCommentId, command.UserId, command.Body, ct);
      return comment;
    }
    catch (KeyNotFoundException)
    {
      return Error.NotFound(
        code: "Comments.ParentNotFound",
        description: $"Parent comment '{command.ParentCommentId}' was not found.");
    }
    catch (ArgumentException)
    {
      return Error.Validation(
        code: "Comments.ReplyDepthUnsupported",
        description: "Replies to replies are not supported.");
    }
  }
}
