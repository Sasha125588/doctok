using Api.Auth;
using Api.Features.Admin.Mdn.Preload;
using Api.Features.Feed;
using Api.Features.Resolve.Mdn;
using Api.Features.Session.Me;
using Api.Features.System.DbPing;
using Api.Features.System.Health;
using Api.Features.Topics;
using Api.Features.Topics._slug_.Links;
using Infrastructure.Cards;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;
using Handler = Api.Features.Resolve.Mdn.Handler;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.Configure<SupabaseJwtOptions>(builder.Configuration.GetSection("Supabase"));

var supabaseJwtOptions =
    builder.Configuration.GetSection("Supabase").Get<SupabaseJwtOptions>()
    ?? throw new InvalidOperationException("Supabase config missing");

var gh = builder.Configuration.GetSection("GitHub").Get<GitHubOptions>()
         ?? throw new InvalidOperationException("GitHub config missing");

var mdnApiOptions = builder.Configuration.GetSection("Mdn").Get<MdnApiOptions>()
                    ?? new MdnApiOptions();

var connStr = builder.Configuration.GetConnectionString("Default")
              ?? throw new InvalidOperationException("ConnectionStrings:Default missing");

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("user_role", "admin"));
});

builder.Services.AddSingleton(mdnApiOptions);

builder.Services.AddHttpClient<MdnApiClient>(client =>
{
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<GitHubTreeClient>(client =>
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

builder.Services.AddSingleton<MdnTreeIndex>();
builder.Services.AddSingleton<MdnContentConverter>();
builder.Services.AddSingleton<MdnIngestionService>();
builder.Services.AddSingleton<MdnSourceJobHandler>();
builder.Services.AddSingleton<ISourceJobHandler>(sp => sp.GetRequiredService<MdnSourceJobHandler>());
builder.Services.AddSingleton<Api.Features.Admin.Mdn.Preload.Handler>();

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

builder.Services.AddSingleton<Handler>();

builder.Services.AddSingleton<JobProcessor>();
builder.Services.AddHostedService<JobRunnerBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseCors();
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
