namespace Api.Features.Feed;

public sealed record Response(IReadOnlyList<FeedItem> Items, string? NextCursor);
