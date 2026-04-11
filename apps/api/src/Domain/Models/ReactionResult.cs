namespace Domain.Models;

public sealed record ReactionResult(
    string MyVote,
    int LikeCount,
    int DislikeCount
);
