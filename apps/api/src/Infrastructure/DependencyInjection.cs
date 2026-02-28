using System.Net.Http.Headers;
using Infrastructure.Jobs;
using Infrastructure.Llm;
using Infrastructure.Persistence.Db;
using Infrastructure.Persistence.Repos.Comments;
using Infrastructure.Persistence.Repos.Feed;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Persistence.Repos.Posts;
using Infrastructure.Persistence.Repos.Raw;
using Infrastructure.Persistence.Repos.Resolve;
using Infrastructure.Persistence.Repos.Sources;
using Infrastructure.Persistence.Repos.Topics;
using Infrastructure.Persistence.Repos.Votes;
using Infrastructure.Posts;
using Infrastructure.Posts.Title;
using Infrastructure.Sources.Common;
using Infrastructure.Sources.GitHub;
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
    var connStr = configuration.GetConnectionString("Default")
                  ?? throw new InvalidOperationException("ConnectionStrings:Default missing");

    var gh = configuration.GetSection("GitHub").Get<GitHubOptions>()
             ?? throw new InvalidOperationException("GitHub config missing");

    var mdnApiOptions = configuration.GetSection("Mdn").Get<MdnApiOptions>()
                        ?? new MdnApiOptions();

    services
      .Configure<OpenRouterOptions>(
        configuration.GetSection("OpenRouter"))
      .Configure<TitleGeneratorOptions>(
        configuration.GetSection("OpenRouter:TitleGenerator"));

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
    services.AddSingleton<TopicReadRepository>();
    services.AddSingleton<TopicLinksRepository>();
    services.AddSingleton<VotesRepository>();
    services.AddSingleton<CommentsRepository>();

    // HTTP clients
    services.AddSingleton(mdnApiOptions);

    services.AddHttpClient<MdnApiClient>(client =>
    {
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    });

    services.AddHttpClient<GitHubTreeClient>(client =>
    {
      client.BaseAddress = new Uri(gh.ApiBaseUrl);

      client.DefaultRequestHeaders.UserAgent.Clear();
      client.DefaultRequestHeaders.UserAgent.Add(
        new ProductInfoHeaderValue(gh.UserAgent, "1.0"));

      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

      client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", gh.Token);
    });

    services
      .AddHttpClient<OpenRouterClient>((sp, client) =>
      {
        var opts = sp.GetRequiredService<IOptions<OpenRouterOptions>>().Value;

        client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
        client.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", opts.ApiKey);

        client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", opts.Referer);
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", opts.AppName);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DocTok/1.0");

        client.Timeout = TimeSpan.FromSeconds(20);
      });

    // Source handlers
    services.AddSingleton<MdnTreeIndex>();
    services.AddSingleton<MdnContentConverter>();
    services.AddSingleton<MdnIngestionService>();
    services.AddSingleton<MdnSourceJobHandler>();
    services.AddSingleton<ISourceJobHandler>(sp => sp.GetRequiredService<MdnSourceJobHandler>());

    // Post generation
    services.AddSingleton<FastPostGenerator>();
    services.AddSingleton<FastPostGenerationService>();
    services.AddSingleton<ITitleGenerator, TitleGenerator>();

    // Background jobs
    services.AddSingleton<JobProcessor>();
    services.AddHostedService<JobRunnerBackgroundService>();

    return services;
  }
}
