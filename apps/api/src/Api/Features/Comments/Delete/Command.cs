namespace Api.Features.Comments.Delete;

public sealed record Command(long CommentId, Guid UserId);
