using Domain.Topics;

namespace Api.Endpoints.Feed.Topics;

public sealed record TopicFeedResponse(IReadOnlyList<TopicFeedPageView> Items, string? NextCursor);
