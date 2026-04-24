using Domain.Posts;

namespace Api.Endpoints.Me.SavedPosts.List;

public sealed record SavedPostsResponse(IReadOnlyList<SavedPostView> Items, string? NextCursor);
