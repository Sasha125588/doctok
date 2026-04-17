using System.Net.Http.Headers;
using Domain.Sources;
using Google.GenAI;
using Infrastructure.Events;
using Infrastructure.Extensions;
using Infrastructure.Jobs;
using Infrastructure.Llm.Abstractions;
using Infrastructure.Llm.Configuration;
using Infrastructure.Llm.Providers.Gemini;
using Infrastructure.Llm.Providers.Local;
using Infrastructure.Llm.Providers.OpenRouter;
using Infrastructure.Llm.Routing;
using Infrastructure.Persistence.ConnectionFactory;
using Infrastructure.Persistence.Repositories;
using Infrastructure.PostGeneration;
using Infrastructure.PostGeneration.Fast;
using Infrastructure.PostGeneration.Llm;
using Infrastructure.Sources.Mdn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure;

public static class InfrastructureServiceRegistration
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    services.AddValidatedOptions<OpenRouterOptions>("OpenRouter");
    services.AddValidatedOptions<GeminiOptions>("Gemini");
    services.AddValidatedOptions<LocalLlmOptions>("LocalLlm");
    services.AddValidatedOptions<LlmProfilesOptions>("LlmProfiles");

    var connStr = configuration.GetConnectionString("Default")
                  ?? throw new InvalidOperationException("ConnectionStrings:Default missing");

    // Database
    services.AddSingleton(_ => NpgsqlDataSource.Create(connStr));
    services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

    // Repositories
    services.AddSingleton<RawDocumentsRepository>();
    services.AddSingleton<RawLinksRepository>();
    services.AddSingleton<TopicsRepository>();
    services.AddSingleton<TopicDocumentsRepository>();
    services.AddSingleton<SourcesRepository>();
    services.AddSingleton<ResolveRepository>();
    services.AddSingleton<JobsRepository>();
    services.AddSingleton<PostsRepository>();
    services.AddSingleton<FeedRepository>();
    services.AddSingleton<TopicFeedRepository>();
    services.AddSingleton<TopicLinksRepository>();
    services.AddSingleton<BaseReactionsRepository>();
    services.AddSingleton<PostReactionsRepository>();
    services.AddSingleton<CommentReactionsRepository>();
    services.AddSingleton<CommentsRepository>();

    services.AddSingleton((sp) =>
    {
      var opts = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;

      return new Client(apiKey: opts.ApiKey);
    });

    services.AddSingleton<GeminiClient>();

    // HTTP clients
    services.AddHttpClient<MdnApiClient>(client =>
    {
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    });

    services.AddHttpClient<MdnSitemapClient>(client =>
    {
      client.BaseAddress = new Uri("https://developer.mozilla.org/");
      client.Timeout = TimeSpan.FromSeconds(60);
    });

    // TODO: Пофіксити lifetime mismatch між singleton LlmRouter і typed(transient) HttpClient
    services.AddHttpClient<OpenRouterClient>((sp, client) =>
    {
      var opts = sp.GetRequiredService<IOptions<OpenRouterOptions>>().Value;

      client.BaseAddress = opts.BaseUrl;
      client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", opts.ApiKey);

      client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", opts.Referer);
      client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", opts.AppName);
      client.DefaultRequestHeaders.UserAgent.ParseAdd("DocTok/1.0");

      client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
    });

    services.AddHttpClient<LocalLlmClient>((sp, client) =>
    {
      var opts = sp.GetRequiredService<IOptions<LocalLlmOptions>>().Value;

      client.BaseAddress = opts.BaseUrl;
      client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);

      if (!string.IsNullOrWhiteSpace(opts.ApiKey))
      {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opts.ApiKey);
      }
    });

    // LLM abstraction
    services.AddSingleton<ILlmRouter, LlmRouter>();

    // Source handlers (keyed by source code for JobProcessor lookup)
    services.AddSingleton<MdnSitemapIndex>();
    services.AddSingleton<MdnContentConverter>();
    services.AddSingleton<MdnIngestionService>();
    services.AddKeyedSingleton<ISourceJobHandler, MdnSourceJobHandler>(SourceCodes.Mdn);

    // Post generation
    services.AddSingleton<MarkdownHtmlRenderer>();
    services.AddSingleton<FastPostGenerator>();
    services.AddSingleton<FastPostGenerationService>();
    services.AddSingleton<LlmPostGenerator>();
    services.AddSingleton<LlmPostGenerationService>();

    // Background jobs
    services.AddSingleton<JobProcessor>();
    services.AddHostedService<JobRunnerBackgroundService>();

    // Events
    services.AddSingleton<TopicGenerationNotifier>();

    return services;
  }
}
