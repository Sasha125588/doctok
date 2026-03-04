using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Tests.Infrastructure;
using Xunit;

namespace Api.Tests.Errors;

public sealed class ErrorContractTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
  private readonly HttpClient client = factory.CreateClient();

  [Fact]
  public async Task ResolveWithoutExternalRefReturnsValidationProblemDetails()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    var requestUri = new Uri("/api/resolve/mdn?lang=ru", UriKind.Relative);
    var response = await client.GetAsync(requestUri, cancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

    var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
    Assert.Equal(400, payload.GetProperty("status").GetInt32());
    Assert.Equal("Bad Request", payload.GetProperty("title").GetString());
    Assert.Equal("externalRef is required", payload.GetProperty("detail").GetString());
    Assert.Equal("https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1", payload.GetProperty("type").GetString());
    Assert.Equal("/api/resolve/mdn", payload.GetProperty("instance").GetString());

    var traceId = payload.GetProperty("traceId").GetString();
    Assert.False(string.IsNullOrWhiteSpace(traceId));

    var errors = payload.GetProperty("errors");
    Assert.Equal(JsonValueKind.Array, errors.ValueKind);
    Assert.Equal("Resolve.ExternalRef.Required", errors[0].GetProperty("code").GetString());
    Assert.Equal("Validation", errors[0].GetProperty("type").GetString());
  }

  [Fact]
  public async Task MeWithInvalidSubReturnsUnauthorizedProblemDetails()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
    request.Headers.Add(TestAuthenticationHandler.HeaderName, TestAuthenticationHandler.InvalidSubMode);

    var response = await client.SendAsync(request, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

    var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
    Assert.Equal(401, payload.GetProperty("status").GetInt32());
    Assert.Equal("Unauthorized", payload.GetProperty("title").GetString());
    Assert.Equal("Missing or invalid 'sub' claim", payload.GetProperty("detail").GetString());
    Assert.Equal("https://www.rfc-editor.org/rfc/rfc9110#section-15.5.2", payload.GetProperty("type").GetString());
    Assert.Equal("/api/me", payload.GetProperty("instance").GetString());

    var traceId = payload.GetProperty("traceId").GetString();
    Assert.False(string.IsNullOrWhiteSpace(traceId));
  }

  [Fact]
  public async Task AdminPreloadWithoutTokenReturnsUnauthorizedProblemDetails()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var request = new HttpRequestMessage(
      HttpMethod.Post,
      new Uri("/api/admin/mdn/preload", UriKind.Relative))
    {
      Content = JsonContent.Create(new { lang = "en", count = 1, }),
    };

    var response = await client.SendAsync(request, cancellationToken);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

    var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
    Assert.Equal(401, payload.GetProperty("status").GetInt32());
    Assert.Equal("Unauthorized", payload.GetProperty("title").GetString());
    Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.2", payload.GetProperty("type").GetString());
    Assert.Equal("/api/admin/mdn/preload", payload.GetProperty("instance").GetString());

    var traceId = payload.GetProperty("traceId").GetString();
    Assert.False(string.IsNullOrWhiteSpace(traceId));
  }

  [Fact]
  public async Task AdminPreloadWithNonAdminUserReturnsForbiddenProblemDetails()
  {
    var cancellationToken = TestContext.Current.CancellationToken;
    using var request = new HttpRequestMessage(
      HttpMethod.Post,
      new Uri("/api/admin/mdn/preload", UriKind.Relative))
    {
      Content = JsonContent.Create(new { lang = "en", count = 1, }),
    };
    request.Headers.Add(TestAuthenticationHandler.HeaderName, TestAuthenticationHandler.UserMode);

    var response = await client.SendAsync(request, cancellationToken);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

    var payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
    Assert.Equal(403, payload.GetProperty("status").GetInt32());
    Assert.Equal("Forbidden", payload.GetProperty("title").GetString());
    Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.4", payload.GetProperty("type").GetString());
    Assert.Equal("/api/admin/mdn/preload", payload.GetProperty("instance").GetString());

    var traceId = payload.GetProperty("traceId").GetString();
    Assert.False(string.IsNullOrWhiteSpace(traceId));
  }
}
