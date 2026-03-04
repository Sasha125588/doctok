using Domain.Models;

namespace Api.Features.Topics;

public sealed record TopicPostsResponse(IReadOnlyList<PostItem> Items);
