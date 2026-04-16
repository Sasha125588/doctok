namespace Api.Endpoints.Feed.Topics;

public sealed record TopicFeedResponse(IReadOnlyList<TopicFeedItem> Items, string? NextCursor);

public sealed record TopicFeedItem(
  string Slug,
  string Title,
  string Lang,
  int PostCount,
  TopicFeedPreview Preview);

public sealed record TopicFeedPreview(
  long PostId,
  string Kind,
  string? Title,
  string Body,
  string BodyHtml);
