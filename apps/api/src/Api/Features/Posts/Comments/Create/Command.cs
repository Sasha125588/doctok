namespace Api.Features.Posts.Comments.Create;

public sealed record Command(long PostId, Guid UserId, string Body);
