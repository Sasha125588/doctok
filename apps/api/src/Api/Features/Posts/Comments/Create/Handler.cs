using Api.Extensions;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Posts.Comments.Create;

public sealed class Handler(CommentsRepository commentsRepo) : IHandler
{
  public async Task<Domain.Models.Comment> Handle(Command cmd, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(cmd.Body))
    {
      throw new ArgumentException("Comment body cannot be empty.");
    }

    if (cmd.Body.Length > 2000)
    {
      throw new ArgumentException("Comment body exceeds 2000 characters.");
    }

    return await commentsRepo.CreateRoot(cmd.PostId, cmd.UserId, cmd.Body.Trim(), ct);
  }
}
