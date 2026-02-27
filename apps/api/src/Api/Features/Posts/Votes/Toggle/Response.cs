namespace Api.Features.Posts.Votes.Toggle;

public sealed record Response(
  string MyVote,     // "like" | "dislike" | "none"
  int LikeCount,
  int DislikeCount
);
