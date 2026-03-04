namespace Domain.Models;

/// <summary>
/// A comment on a post.
/// </summary>
public sealed record Comment(
    long Id,
    long PostId,
    Guid UserId,
    long? ParentCommentId,
    string Body,
    DateTimeOffset CreatedAt,
    bool IsDeleted
);
