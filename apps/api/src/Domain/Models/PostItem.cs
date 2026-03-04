namespace Domain.Models;

/// <summary>
/// A post as it appears in a feed or topic view, with aggregated counts and user vote.
/// </summary>
public sealed record PostItem(
    long Id,
    string Kind,
    string Title,
    string Body,
    int Position,
    int LikeCount,
    int DislikeCount,
    int CommentCount,
    string TopicSlug,
    string TopicTitle,
    string MyVote,
    double? Popularity
);
