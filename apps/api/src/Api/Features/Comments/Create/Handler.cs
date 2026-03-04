using Api.Extensions;
using ErrorOr;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Create;

public sealed class Handler(CommentsRepository commentsRepo) : IHandler
{
  public async Task<ErrorOr<Domain.Models.Comment>> Handle(Command command, CancellationToken ct)
  {
    try
    {
      var comment = await commentsRepo.CreateRoot(command.PostId, command.UserId, command.Body, ct);
      return comment;
    }
    catch (KeyNotFoundException)
    {
      return Error.NotFound(
        code: "Comments.PostNotFound",
        description: $"Post '{command.PostId}' was not found.");
    }
  }
}
