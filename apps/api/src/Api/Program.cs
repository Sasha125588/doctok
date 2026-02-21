using Api.Auth;
using Api.Features.Admin.Mdn.Preload;
using Api.Features.Feed;
using Api.Features.Resolve.Mdn;
using Api.Features.Session.Me;
using Api.Features.System.DbPing;
using Api.Features.System.Health;
using Api.Features.Topics;
using Infrastructure.Cards;
using Infrastructure.Persistence.Db;
using Infrastructure.Persistence.Repos.Cards;
using Infrastructure.Persistence.Repos.Feed;
using Infrastructure.Persistence.Repos.Jobs;
using Infrastructure.Persistence.Repos.Raw;
using Infrastructure.Persistence.Repos.Resolve;
using Infrastructure.Persistence.Repos.Sources;
using Infrastructure.Persistence.Repos.Topics;
using Infrastructure.Sources.GitHub;
using Infrastructure.Sources.Mdn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<SupabaseJwtOptions>(builder.Configuration.GetSection("Supabase"));

var supabaseJwtOptions =
    builder.Configuration.GetSection("Supabase").Get<SupabaseJwtOptions>()
    ?? throw new InvalidOperationException("Supabase config missing");

var gh = builder.Configuration.GetSection("GitHub").Get<GitHubOptions>()
         ?? throw new InvalidOperationException("GitHub config missing");

var mdnApiOptions = builder.Configuration.GetSection("Mdn").Get<MdnApiOptions>()
                    ?? new MdnApiOptions();

var connStr = builder.Configuration["Database:ConnectionString"]
              ?? throw new InvalidOperationException("Database:ConnectionString missing");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = true;
        options.MetadataAddress = supabaseJwtOptions.MetadataUrl;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = supabaseJwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = supabaseJwtOptions.JwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),

            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton(gh);
builder.Services.AddSingleton(mdnApiOptions);

builder.Services.AddHttpClient<MdnApiClient>();
builder.Services.AddSingleton<MdnApiClient>();

builder.Services.AddHttpClient<GitHubTreeClient>();
builder.Services.AddSingleton<GitHubTreeClient>();

builder.Services.AddSingleton<MdnTreeIndex>();
builder.Services.AddSingleton<MdnContentConverter>();
builder.Services.AddSingleton<MdnIngestionService>();
builder.Services.AddSingleton<PreloadMdnHandler>();

builder.Services.AddSingleton<RawDocumentsRepository>();
builder.Services.AddSingleton<RawLinksRepository>();
builder.Services.AddSingleton<TopicsRepository>();
builder.Services.AddSingleton<TopicDocumentsRepository>();
builder.Services.AddSingleton<SourcesRepository>();
builder.Services.AddSingleton<ResolveRepository>();
builder.Services.AddSingleton<JobsRepository>();
builder.Services.AddSingleton<CardsRepository>();
builder.Services.AddSingleton<FeedRepository>();
builder.Services.AddSingleton<TopicReadRepository>();
builder.Services.AddSingleton<TopicLinksRepository>();

builder.Services.AddSingleton<FastCardGenerator>();
builder.Services.AddSingleton<FastCardGenerationService>();

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(connStr));
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddSingleton<ResolveMdnHandler>();

builder.Services.AddSingleton<JobProcessor>();
builder.Services.AddHostedService<JobRunnerBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealth();
app.MapMe();
app.MapDbPing();
app.MapFeed();
app.MapTopics();
app.MapTopicsLinks();
app.MapResolveMdn();
app.MapAdminMdnPreload();

app.Run();
