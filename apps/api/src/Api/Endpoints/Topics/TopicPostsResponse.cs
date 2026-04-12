using Domain.Models;

namespace Api.Endpoints.Topics;

public sealed record TopicPostsResponse(IReadOnlyList<PostItem> Items);
