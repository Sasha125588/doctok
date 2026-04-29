namespace Domain.Comments;

public sealed record CommentView(
  long Id,
  long PostId,
  Guid UserId,
  long? ParentCommentId,
  string Body,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt,
  DateTimeOffset? DeletedAt,
  int LikeCount,
  int DislikeCount,
  int ReplyCount
);
