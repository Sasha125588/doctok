namespace Api.Endpoints.Comments.Delete;

public sealed record Command(long CommentId, Guid UserId);
