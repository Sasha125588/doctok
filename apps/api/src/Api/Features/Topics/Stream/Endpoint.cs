using System.Text.Json;
using Api.Extensions;
using Domain.Common;
using Domain.EventsArgs;
using Infrastructure.Events;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics.Stream;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/topics/stream", async (
      HttpContext context,
      TopicGenerationEvents events,
      TopicReadRepository topicRepo,
      [AsParameters] TopicsStreamQueryParams query,
      CancellationToken ct) =>
    {
      var slug = (query.Slug ?? string.Empty).Trim().Trim('/');
      if (string.IsNullOrWhiteSpace(slug))
      {
        await Results.ValidationProblem(new Dictionary<string, string[]>
        {
          ["slug"] = ["slug is required"],
        }).ExecuteAsync(context);

        return;
      }

      var lang = LanguageHelpers.NormalizeLang(query.Lang ?? "en");

      context.Response.Headers.CacheControl = "no-cache";
      context.Response.Headers.Connection = "keep-alive";
      context.Response.ContentType = "text/event-stream";

      var tcs = new TaskCompletionSource<TopicGenerationEventArgs>();

      void Handler(object? _, TopicGenerationEventArgs args)
      {
        if (lang == args.Lang && slug == args.Slug)
        {
          tcs.TrySetResult(args);
        }
      }

      events.GenerationEvent += Handler;

      var hasPosts = await topicRepo.PostsExistForTopic(slug, lang, ct);

      try
      {
        var message = await tcs.Task.WaitAsync(ct);
        var serializedMsg = JsonSerializer.Serialize(message);

        if (hasPosts)
        {
          await context.Response.WriteAsync("event: topic-ready\n", ct);
          await context.Response.WriteAsync($"data: {serializedMsg}\n\n", ct);

          await context.Response.Body.FlushAsync(ct);

          return;
        }

        switch (message.Status)
        {
          case TopicGenerationStatus.Ready:
            await context.Response.WriteAsync("event: topic-ready\n", ct);
            await context.Response.WriteAsync($"data: {serializedMsg}\n\n", ct);
            break;
          case TopicGenerationStatus.Failed:
            await context.Response.WriteAsync("event: topic-failed\n", ct);
            await context.Response.WriteAsync($"data: {serializedMsg}\n\n", ct);
            break;
        }

        await context.Response.Body.FlushAsync(ct);
      }
      catch (OperationCanceledException)
      {
      }
      finally
      {
        events.GenerationEvent -= Handler;
      }
    });
  }
}
