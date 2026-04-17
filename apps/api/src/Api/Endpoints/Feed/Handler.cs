using Api.Extensions;
using Domain.Posts;
using Domain.Shared;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Feed;

public sealed class Handler(FeedRepository feedRepo) : IHandler
{
    public async Task<FeedResponse> Handle(Query query, CancellationToken ct)
    {
        var take = Math.Clamp(query.Limit ?? 20, 1, 50);
        var lang = LanguageHelpers.NormalizeLang(query.Lang);
        var cursor = CursorCodec.Decode<FeedCursor>(query.Cursor);

        var page = await feedRepo.GetPage(cursor, query.UserId, lang, take + 1, ct);

        var hasNextPage = page.Count > take;
        var items = hasNextPage ? page.Take(take).ToList() : page;

        var nextCursor = hasNextPage
          ? CursorCodec.Encode(ToCursor(items[^1]))
          : null;

        return new FeedResponse(items, nextCursor);
    }

    private static FeedCursor ToCursor(TopicPostView item)
      => new(item.Popularity, item.Id, item.CreatedAt);
}
