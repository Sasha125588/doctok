namespace Domain.Sources;

public sealed class Source
{
  public long Id { get; set; }

  public string Code { get; set; } = null!;
  public string Title { get; set; } = null!;
}
