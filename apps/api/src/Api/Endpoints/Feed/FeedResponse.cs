using Domain.Posts;

namespace Api.Endpoints.Feed;

public sealed record FeedResponse(IReadOnlyList<TopicPostView> Items, string? NextCursor);
