using Domain.Models;

namespace Api.Features.Feed;

public sealed record FeedResponse(IReadOnlyList<PostItem> Items, string? NextCursor);
