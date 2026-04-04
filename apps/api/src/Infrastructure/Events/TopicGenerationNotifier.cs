using Domain.Events.Topic;

namespace Infrastructure.Events;

public sealed class TopicGenerationNotifier
{
  public event Action<TopicGenerationEvent>? GenerationEvent;

  public void NotifyReady(string slug, string lang)
    => GenerationEvent?.Invoke(
      new TopicGenerationEvent(slug, lang, TopicGenerationStatus.Ready));

  public void NotifyFailed(string slug, string lang, string? error)
    => GenerationEvent?.Invoke(
      new TopicGenerationEvent(slug, lang, TopicGenerationStatus.Failed, error));
}