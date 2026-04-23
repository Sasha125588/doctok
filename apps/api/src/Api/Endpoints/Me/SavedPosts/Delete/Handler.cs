using Api.Extensions;
using ErrorOr;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Me.SavedPosts.Delete;

public sealed class Handler(SavedPostsRepository savedPostsRepository) : IHandler
{
  public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
  {
    await savedPostsRepository.Remove(
      command.UserId,
      command.PostId,
      ct);

    return Result.Success;
  }
}
