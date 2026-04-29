namespace Api.Endpoints.Topics;

public sealed record Query(string Slug, string? Lang, int? Limit, string? Cursor, Guid? UserId);
