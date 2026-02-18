namespace Api.Features.Session.Me;

public sealed record Response(Guid UserId, string? Email);