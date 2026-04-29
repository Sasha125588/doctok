using Domain.Reactions;

namespace Domain.Posts;

public sealed record TopicPostMetaView(
  long Id,
  string Kind,
  string Title,
  int Position,
  int LikeCount,
  int DislikeCount,
  int CommentCount,
  string TopicSlug,
  string TopicTitle,
  ReactionValue MyVote,
  double? Popularity,
  DateTime CreatedAt,
  bool IsSaved
);
