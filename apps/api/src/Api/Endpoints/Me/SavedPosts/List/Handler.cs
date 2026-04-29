using Api.Extensions;
using Domain.Posts;
using Domain.Shared;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Me.SavedPosts.List;

public sealed class Handler(SavedPostsRepository savedPostsRepository) : IHandler
{
  public async Task<SavedPostsResponse> Handle(Query query, CancellationToken ct)
  {
    var take = Math.Clamp(query.Limit ?? 5, 1, 50);
    var cursor = CursorCodec.Decode<SavedPostsCursor>(query.Cursor);

    var page = await savedPostsRepository.List(
      cursor,
      query.UserId,
      take + 1,
      ct);

    var pageResult = CursorPage.From(page, take, ToCursor);

    return new SavedPostsResponse(pageResult.Items, pageResult.NextCursor);
  }

  private static SavedPostsCursor ToCursor(SavedPostView item)
    => new(item.PostId, item.SavedAt);
}
