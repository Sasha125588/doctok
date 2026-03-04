using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Api.Tests.Infrastructure;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    builder.ConfigureAppConfiguration((_, configurationBuilder) =>
    {
      configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Supabase:ProjectRef"] = "test-project",
        ["GitHub:Token"] = "test-token",
        ["OpenRouter:ApiKey"] = "test-api-key",
        ["OpenRouter:Referer"] = "http://localhost:5005",
        ["OpenRouter:AppName"] = "DocTok.Tests",
        ["OpenRouter:TitleGenerator:Model"] = "test-model",
        ["ConnectionStrings:Default"] = "Host=localhost;Port=5432;Username=test;Password=test;Database=test_db",
      });
    });

    builder.ConfigureTestServices(services =>
    {
      services.RemoveAll<IHostedService>();

      services
        .AddAuthentication(TestAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
          TestAuthenticationHandler.SchemeName,
          _ => { });
    });
  }
}
