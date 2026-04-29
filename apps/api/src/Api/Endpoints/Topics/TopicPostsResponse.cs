using Domain.Posts;

namespace Api.Endpoints.Topics;

public sealed record TopicPostsResponse(IReadOnlyList<TopicPostMetaView> Items, string? NextCursor);
