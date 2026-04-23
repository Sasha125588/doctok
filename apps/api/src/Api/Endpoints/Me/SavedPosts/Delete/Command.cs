namespace Api.Endpoints.Me.SavedPosts.Delete;

public sealed record Command(Guid UserId, long PostId);
