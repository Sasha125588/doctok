using Api.Auth;
using Api.Features.Resolve.Mdn;
using Api.Features.Session.Me;
using Api.Features.System.DbPing;
using Api.Features.System.Health;
using DocTok.Infrastructure.Sources.Mdn;
using Domain.Rules;
using Infrastructure.Persistence.Db;
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

var mdnTar = builder.Configuration.GetSection("MdnTarball").Get<MdnTarballOptions>()
             ?? throw new InvalidOperationException("MdnTarball config missing");

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

mdnTar.DataRoot = PathRules.GetMdnCacheRoot();
builder.Services.AddSingleton(mdnTar);

builder.Services.AddHttpClient<GitHubTarballClient>();
builder.Services.AddSingleton<GitHubTarballClient>();

builder.Services.AddSingleton<MdnArchiveManager>();
builder.Services.AddSingleton<MdnIndex>();
builder.Services.AddSingleton<MdnRawParser>();
builder.Services.AddSingleton<MdnLinkExtractor>();
builder.Services.AddSingleton<MdnIngestionService>();

builder.Services.AddSingleton<RawDocumentsRepository>();
builder.Services.AddSingleton<RawLinksRepository>();
builder.Services.AddSingleton<TopicsRepository>();
builder.Services.AddSingleton<TopicDocumentsRepository>();
builder.Services.AddSingleton<SourcesRepository>();
builder.Services.AddSingleton<ResolveRepository>();
builder.Services.AddSingleton<JobsRepository>();

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

app.MapResolveMdn();

app.Run();
