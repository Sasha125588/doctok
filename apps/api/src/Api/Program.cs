using Api;
using Api.Features.Admin.Mdn.Preload;
using Api.Features.Feed;
using Api.Features.Resolve.Mdn;
using Api.Features.Session.Me;
using Api.Features.System.DbPing;
using Api.Features.System.Health;
using Api.Features.Topics;
using Api.Features.Topics._slug_.Links;
using Infrastructure;
using Scalar.AspNetCore;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

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
