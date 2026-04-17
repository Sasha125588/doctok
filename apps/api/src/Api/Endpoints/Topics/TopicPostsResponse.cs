using Domain.Posts;

namespace Api.Endpoints.Topics;

public sealed record TopicPostsResponse(IReadOnlyList<TopicPostView> Items, string? NextCursor);
