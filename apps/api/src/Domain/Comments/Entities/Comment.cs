namespace Domain.Comments;

public sealed class Comment
{
  public long Id { get; set; }
  public long PostId { get; set; }
  public Guid UserId { get; set; }

  public long? ParentCommentId { get; set; }

  public string Body { get; set; } = null!;

  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
  public DateTimeOffset? DeletedAt { get; set; }

  public int LikeCount { get; set; }
  public int DislikeCount { get; set; }
}
