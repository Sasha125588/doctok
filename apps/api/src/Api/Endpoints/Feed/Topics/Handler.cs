using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Feed.Topics;

public sealed class Handler(TopicFeedRepository topicFeedRepo) : IHandler
{
  public async Task<TopicFeedResponse> Handle(Query query, CancellationToken ct)
  {
    var take = Math.Clamp(query.Limit ?? 20, 1, 50);
    var lang = LanguageHelpers.NormalizeLang(query.Lang);
    var cursor = CursorCodec.Decode<FeedCursor>(query.Cursor);

    var page = await topicFeedRepo.GetPage(cursor, lang, take, ct);
    var items = page
      .Select(item => new TopicFeedItem(
        item.Slug,
        item.Title,
        item.Lang,
        item.PostCount,
        new TopicFeedPreview(item.PreviewPostId, item.PreviewKind, item.PreviewTitle, item.PreviewBody)))
      .ToList();

    var nextCursor = page.Count == take
      ? CursorCodec.Encode(new FeedCursor(page[^1].Popularity, page[^1].Id))
      : null;

    return new TopicFeedResponse(items, nextCursor);
  }
}
