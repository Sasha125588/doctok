namespace Domain.Models;

public sealed record PostItem(
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
    string MyVote,
    double? Popularity
);
