namespace Api.Features.Feed;

public sealed record FeedResponse(IReadOnlyList<FeedItem> Items, string? NextCursor);
