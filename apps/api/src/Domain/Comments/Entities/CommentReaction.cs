using Domain.Reactions;

namespace Domain.Comments;

public sealed class CommentReaction
{
  public long CommentId { get; set; }
  public Guid UserId { get; set; }

  public ReactionValue Value { get; set; }

  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}
