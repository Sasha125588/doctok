using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Api.Extensions;
using Domain.Common;
using Domain.EventsArgs;
using Infrastructure.Events;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics.Stream;

public sealed class Endpoint : IEndpoint
{
  private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/topics/stream", (
      TopicGenerationEvents events,
      TopicReadRepository topicRepo,
      [AsParameters] TopicsStreamQueryParams query,
      CancellationToken ct) =>
    {
      var slug = query.Slug!.Trim().Trim('/').ToLowerInvariant();
      var lang = LanguageHelpers.NormalizeLang(query.Lang ?? "en");

      return Results.ServerSentEvents(StreamAsync(events, topicRepo, slug, lang, ct));
    });
  }

  private static async IAsyncEnumerable<SseItem<TopicGenerationEventArgs>> StreamAsync(
    TopicGenerationEvents events,
    TopicReadRepository topicRepo,
    string slug,
    string lang,
    [EnumeratorCancellation] CancellationToken ct)
  {
    var result = await WaitForTopicAsync(events, topicRepo, slug, lang, ct);

    yield return result;
  }

  private static async Task<SseItem<TopicGenerationEventArgs>> WaitForTopicAsync(
    TopicGenerationEvents events,
    TopicReadRepository topicRepo,
    string slug,
    string lang,
    CancellationToken ct)
  {
    if (await topicRepo.PostsExistForTopic(slug, lang, ct))
      return SseReady(slug, lang);

    var tcs = new TaskCompletionSource<TopicGenerationEventArgs>(
      TaskCreationOptions.RunContinuationsAsynchronously);

    void Handler(object? _, TopicGenerationEventArgs args)
    {
      if (args.Slug == slug && args.Lang == lang)
        tcs.TrySetResult(args);
    }

    events.GenerationEvent += Handler;

    try
    {
      if (await topicRepo.PostsExistForTopic(slug, lang, ct))
        tcs.TrySetResult(new TopicGenerationEventArgs(slug, lang, TopicGenerationStatus.Ready));

      using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
      cts.CancelAfter(Timeout);

      var message = await tcs.Task.WaitAsync(cts.Token);

      var eventName = message.Status == TopicGenerationStatus.Ready
        ? "topic-ready"
        : "topic-failed";

      return new SseItem<TopicGenerationEventArgs>(message, eventName);
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
      return new SseItem<TopicGenerationEventArgs>(
        new TopicGenerationEventArgs(slug, lang, TopicGenerationStatus.Failed, "timeout"),
        "topic-timeout");
    }
    finally
    {
      events.GenerationEvent -= Handler;
    }
  }

  private static SseItem<TopicGenerationEventArgs> SseReady(string slug, string lang) =>
    new(new TopicGenerationEventArgs(slug, lang, TopicGenerationStatus.Ready), "topic-ready");
}
