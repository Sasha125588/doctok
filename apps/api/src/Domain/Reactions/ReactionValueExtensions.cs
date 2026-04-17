namespace Domain.Reactions;

public static class ReactionValueExtensions
{
  public static string ToStorageValue(this ReactionValue value)
    => value switch
    {
      ReactionValue.Like => "like",
      ReactionValue.Dislike => "dislike",
      ReactionValue.None => "none",
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported vote value."),
    };
}
