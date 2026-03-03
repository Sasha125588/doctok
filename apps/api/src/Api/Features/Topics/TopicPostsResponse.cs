namespace Api.Features.Topics;

public sealed record TopicPostsResponse(IReadOnlyList<TopicPostItem> Items);
