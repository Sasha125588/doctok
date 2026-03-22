using Domain.EventsArgs;

namespace Infrastructure.Events;

public sealed class TopicGenerationEvents
{
  public event EventHandler<TopicGenerationEventArgs>? GenerationEvent;

  public void NotifyReady(string slug, string lang)
    => GenerationEvent?.Invoke(this,
      new TopicGenerationEventArgs(slug, lang, TopicGenerationStatus.Ready));

  public void NotifyFailed(string slug, string lang, string? error)
    => GenerationEvent?.Invoke(this,
      new TopicGenerationEventArgs(slug, lang, TopicGenerationStatus.Failed, error));
}