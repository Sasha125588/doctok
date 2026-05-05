using Api.Extensions;
using ErrorOr;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Me.SavedPosts.Clear;

public sealed class Handler(SavedPostsRepository savedPostsRepository) : IHandler
{
  public async Task<ErrorOr<int>> Handle(Command command, CancellationToken ct)
  {
    var affected = await savedPostsRepository.Clear(
      command.UserId,
      ct);

    return affected;
  }
}
