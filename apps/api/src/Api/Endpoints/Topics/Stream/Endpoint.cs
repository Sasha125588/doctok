using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Api.Extensions;
using Domain.Shared;
using Domain.Sources;
using Domain.Topics.Events;
using Infrastructure.Events;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Topics.Stream;

public sealed class Endpoint : IEndpoint
{
  private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/topics/stream", (
        TopicGenerationNotifier notifier,
        TopicsRepository topicRepo,
        [AsParameters] TopicsStreamQueryParams query,
        CancellationToken ct) =>
      {
        var slug = ExternalRefHelpers.Normalize(query.Slug);
        var lang = LanguageHelpers.NormalizeLang(query.Lang);

        return Results.ServerSentEvents(StreamAsync(notifier, topicRepo, slug, lang, ct));
      })
      .WithTags("Topics")
      .WithSummary("Streams topic generation status")
      .WithDescription("Returns a Server-Sent Events stream with topic-ready, topic-failed, and topic-timeout events.")
      .WithName("TopicsStream")
      .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
      .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }

  private static async IAsyncEnumerable<SseItem<TopicGenerationEvent>> StreamAsync(
    TopicGenerationNotifier notifier,
    TopicsRepository topicRepo,
    string slug,
    string lang,
    [EnumeratorCancellation] CancellationToken ct)
  {
    var result = await WaitForTopicAsync(notifier, topicRepo, slug, lang, ct);

    yield return result;
  }

  private static async Task<SseItem<TopicGenerationEvent>> WaitForTopicAsync(
    TopicGenerationNotifier notifier,
    TopicsRepository topicRepo,
    string slug,
    string lang,
    CancellationToken ct)
  {
    if (await topicRepo.PostsExistForTopic(slug, lang, ct))
      return SseReady(slug, lang);

    var tcs = new TaskCompletionSource<TopicGenerationEvent>(
      TaskCreationOptions.RunContinuationsAsynchronously);

    void Handler(TopicGenerationEvent message)
    {
      if (message.Slug == slug && message.Lang == lang)
        tcs.TrySetResult(message);
    }

    notifier.GenerationEvent += Handler;

    try
    {
      if (await topicRepo.PostsExistForTopic(slug, lang, ct))
        tcs.TrySetResult(new TopicGenerationEvent(slug, lang, TopicGenerationStatus.Ready));

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
      cts.CancelAfter(Timeout);

      var message = await tcs.Task.WaitAsync(cts.Token);

      var eventName = message.Status == TopicGenerationStatus.Ready
        ? "topic-ready"
        : "topic-failed";

      return new SseItem<TopicGenerationEvent>(message, eventName);
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
      return new SseItem<TopicGenerationEvent>(
        new TopicGenerationEvent(slug, lang, TopicGenerationStatus.Failed, "timeout"),
        "topic-timeout");
    }
    finally
    {
      notifier.GenerationEvent -= Handler;
    }
  }

  private static SseItem<TopicGenerationEvent> SseReady(string slug, string lang) =>
    new(new TopicGenerationEvent(slug, lang, TopicGenerationStatus.Ready), "topic-ready");
}
