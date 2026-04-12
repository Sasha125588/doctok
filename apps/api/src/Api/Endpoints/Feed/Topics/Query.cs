namespace Api.Endpoints.Feed.Topics;

public sealed record Query(string? Cursor, string? Lang, int? Limit);
