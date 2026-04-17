namespace Domain.Topics;

public class Topic
{
  public long Id { get; set; }

  public string Slug { get; set; } = null!;
  public string Title { get; set; } = null!;

  public DateTimeOffset CreatedAt { get; set; }
}
