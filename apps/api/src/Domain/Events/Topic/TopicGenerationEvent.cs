namespace Domain.Events.Topic;

public sealed record TopicGenerationEvent(
  string Slug,
  string Lang,
  TopicGenerationStatus Status,
  string? Error = null);