namespace Api.Features.Topics;

public sealed record Query(string Slug, string? Lang, Guid? UserId);
