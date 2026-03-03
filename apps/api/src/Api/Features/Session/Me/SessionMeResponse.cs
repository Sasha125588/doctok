namespace Api.Features.Session.Me;

public sealed record SessionMeResponse(Guid UserId, string? Email);
