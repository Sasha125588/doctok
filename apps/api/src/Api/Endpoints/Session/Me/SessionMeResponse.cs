namespace Api.Endpoints.Session.Me;

public sealed record SessionMeResponse(Guid UserId, string? Email);
