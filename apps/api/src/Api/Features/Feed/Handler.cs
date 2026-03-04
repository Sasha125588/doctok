using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repos.Feed;

namespace Api.Features.Feed;

public sealed class Handler(FeedRepository feedRepo) : IHandler
{
    public async Task<FeedResponse> Handle(Query query, CancellationToken ct)
    {
        var take = Math.Clamp(query.Limit ?? 20, 1, 50);
        var lang = LanguageHelpers.NormalizeLang(query.Lang ?? "en");
        var cursor = CursorCodec.Decode(query.Cursor);

        var items = await feedRepo.GetPage(cursor, query.UserId, lang, take, ct);

        var nextCursor = items.Count == take
          ? CursorCodec.Encode(new FeedCursor(items[^1].Popularity, items[^1].Id))
          : null;

        return new FeedResponse(items, nextCursor);
    }
}
