namespace Api.Endpoints.Comments.Replies.Create;

public sealed record Command(long ParentCommentId, Guid UserId, string Body);
