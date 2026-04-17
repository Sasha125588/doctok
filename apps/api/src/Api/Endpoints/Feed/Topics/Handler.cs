using Api.Extensions;
using Domain.Shared;
using Domain.Topics;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Feed.Topics;

public sealed class Handler(TopicFeedRepository topicFeedRepo) : IHandler
{
  public async Task<TopicFeedResponse> Handle(Query query, CancellationToken ct)
  {
    var take = Math.Clamp(query.Limit ?? 5, 1, 50);
    var lang = LanguageHelpers.NormalizeLang(query.Lang);
    var cursor = CursorCodec.Decode<FeedCursor>(query.Cursor);

    var page = await topicFeedRepo.GetPage(cursor, lang, take + 1, ct);

    var hasNextPage = page.Count > take;
    var items = hasNextPage ? page.Take(take).ToList() : page;

    var nextCursor = hasNextPage
      ? CursorCodec.Encode(ToCursor(items[^1]))
      : null;

    return new TopicFeedResponse(items, nextCursor);
  }

  private static FeedCursor ToCursor(TopicFeedPageView item)
    => new(item.Popularity, item.Id, item.CreatedAt);
}
