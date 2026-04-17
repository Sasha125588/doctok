namespace Domain.Posts;

public class Post
{
  public long Id { get; set; }

  public long TopicId { get; set; }
  public long RawDocumentId { get; set; }

  public string Lang { get; set; } = null!;
  public PostKind Kind { get; set; }

  public string? Title { get; set; }
  public string Body { get; set; } = null!;
  public string BodyHtml { get; set; } = string.Empty;

  public int Position { get; set; }

  public DateTimeOffset CreatedAt { get; set; }

  public int LikeCount { get; set; }
  public int DislikeCount { get; set; }
  public int CommentCount { get; set; }

  public short GenerationLevel { get; set; }
}
