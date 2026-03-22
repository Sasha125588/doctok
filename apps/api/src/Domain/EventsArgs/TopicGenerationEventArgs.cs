namespace Domain.EventsArgs;

public enum TopicGenerationStatus
{
  Ready,
  Failed
}

public sealed class TopicGenerationEventArgs(
  string slug,
  string lang,
  TopicGenerationStatus status,
  string? error = null)
  : EventArgs
{
  public string Slug { get; init; } = slug;

  public string Lang { get; init; } = lang;

  public TopicGenerationStatus Status { get; init; } = status;

  public string? Error { get; init; } = error;
}
