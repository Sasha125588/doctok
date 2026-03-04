namespace Domain.Models;

/// <summary>
/// Result of a vote toggle operation.
/// </summary>
public sealed record VoteResult(
    string MyVote,
    int LikeCount,
    int DislikeCount
);
