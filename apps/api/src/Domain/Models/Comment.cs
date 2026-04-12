namespace Domain.Models;

public sealed record Comment(
    long Id,
    long PostId,
    Guid UserId,
    long? ParentCommentId,
    string Body,
    int LikeCount,
    int DislikeCount,
    DateTimeOffset CreatedAt,
    bool IsDeleted
);
