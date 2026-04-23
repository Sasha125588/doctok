namespace Api.Endpoints.Me.SavedPosts.List;

public sealed record Query(string? Cursor, Guid UserId, int? Limit);
