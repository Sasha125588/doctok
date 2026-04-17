namespace Domain.Posts;

public static class PostKindExtensions
{
  public static string ToStorageValue(this PostKind kind)
    => kind switch
    {
      PostKind.Summary => "summary",
      PostKind.Concept => "concept",
      PostKind.Example => "example",
      PostKind.Tip     => "tip",
      _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported post kind."),
    };
}
