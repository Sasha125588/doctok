namespace Domain.Common;

public static class VoteValueExtensions
{
  public static string ToStorageValue(this VoteValue value)
    => value switch
    {
      VoteValue.Like => "like",
      VoteValue.Dislike => "dislike",
      _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported vote value."),
    };
}
