using System.Text.Json;

namespace Domain.Jobs;

public sealed class Job
{
  public long Id { get; set; }

  public string JobType { get; set; } = null!;
  public string JobKey { get; set; } = null!;

  public JsonDocument Payload { get; set; } = null!;

  public JobStatus Status { get; set; }

  public int Attempts { get; set; }

  public DateTimeOffset CreatedAt { get; set; }
  public DateTimeOffset UpdatedAt { get; set; }

  public string? LastError { get; set; }
  public DateTimeOffset NextAttemptAt { get; set; }
}
