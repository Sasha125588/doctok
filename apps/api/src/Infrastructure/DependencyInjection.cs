using Infrastructure.Cards;
using Infrastructure.Jobs;
using Infrastructure.Persistence.Db;
using Infrastructure.Persistence.Repos.Cards;
using Infrastructure.Persistence.Repos.Feed;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Persistence.Repos.Raw;
using Infrastructure.Persistence.Repos.Resolve;
using Infrastructure.Persistence.Repos.Sources;
using Infrastructure.Persistence.Repos.Topics;
using Infrastructure.Sources.Common;
using Infrastructure.Sources.GitHub;
using Infrastructure.Sources.Mdn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    services.AddSingleton<CardsRepository>();
    services.AddSingleton<FeedRepository>();
    services.AddSingleton<TopicReadRepository>();
    services.AddSingleton<TopicLinksRepository>();

    // HTTP clients
    services.AddSingleton(mdnApiOptions);

    services.AddHttpClient<MdnApiClient>(client =>
    {
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    });

    services.AddHttpClient<GitHubTreeClient>(client =>
    {
      client.BaseAddress = new Uri(gh.ApiBaseUrl);

      client.DefaultRequestHeaders.UserAgent.Clear();
      client.DefaultRequestHeaders.UserAgent.Add(
        new System.Net.Http.Headers.ProductInfoHeaderValue(gh.UserAgent, "1.0"));

      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

      client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gh.Token);
    });

    // Source handlers
    services.AddSingleton<MdnTreeIndex>();
    services.AddSingleton<MdnContentConverter>();
    services.AddSingleton<MdnIngestionService>();
    services.AddSingleton<MdnSourceJobHandler>();
    services.AddSingleton<ISourceJobHandler>(sp => sp.GetRequiredService<MdnSourceJobHandler>());

    // Card generation
    services.AddSingleton<FastCardGenerator>();
    services.AddSingleton<FastCardGenerationService>();

    // Background jobs
    services.AddSingleton<JobProcessor>();
    services.AddHostedService<JobRunnerBackgroundService>();

    return services;
  }
}
