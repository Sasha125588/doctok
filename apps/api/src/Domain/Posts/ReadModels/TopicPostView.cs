using Domain.Reactions;

namespace Domain.Posts;

public sealed record TopicPostView(
  long Id,
  string Kind,
  string Title,
  string Body,
  string BodyHtml,
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
