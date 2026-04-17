namespace Domain.Reactions;

public sealed record ReactionView(
    ReactionValue MyVote,
    int LikeCount,
    int DislikeCount
);
