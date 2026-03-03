namespace Api.Features.Posts.Votes.Toggle;

public sealed record TogglePostVoteResponse(
  string MyVote,     // "like" | "dislike" | "none"
  int LikeCount,
  int DislikeCount
);
