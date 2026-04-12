namespace Api.Endpoints.Topics;

public sealed record Query(string Slug, string? Lang, Guid? UserId);
