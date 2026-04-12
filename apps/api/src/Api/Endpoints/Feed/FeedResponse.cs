using Domain.Models;

namespace Api.Endpoints.Feed;

public sealed record FeedResponse(IReadOnlyList<PostItem> Items, string? NextCursor);
