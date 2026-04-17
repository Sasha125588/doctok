using Domain.Reactions;

namespace Domain.Posts;

public class PostReaction
{
  public long PostId { get; set; }
  public Guid UserId { get; set; }

  public ReactionValue Value { get; set; }

  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }
}
