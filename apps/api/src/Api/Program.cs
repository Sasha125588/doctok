using Api;
using Api.Extensions;
using Infrastructure;
using Scalar.AspNetCore;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddWebServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
      options.WithTitle("DocTok");
      options.DarkMode = true;
      options.AddPreferredSecuritySchemes("BearerAuth");
    });
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
