using Domain.Reactions;

namespace Domain.Comments;

public sealed record CommentView(
  long Id,
  long PostId,
  Guid UserId,
  long? ParentCommentId,
  string Body,
  DateTime CreatedAt,
  DateTime UpdatedAt,
  DateTime? DeletedAt,
  int LikeCount,
  int DislikeCount,
  int ReplyCount,
  ReactionValue MyVote
);
