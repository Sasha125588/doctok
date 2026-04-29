namespace Api.Endpoints.Feed;

public sealed record Query(string? Cursor, string? Lang, int? Limit, string? Variant, Guid? UserId);
