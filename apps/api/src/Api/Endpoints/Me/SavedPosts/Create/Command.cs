namespace Api.Endpoints.Me.SavedPosts.Create;

public sealed record Command(Guid UserId, long PostId);
