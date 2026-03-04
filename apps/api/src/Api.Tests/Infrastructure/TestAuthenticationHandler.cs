using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Tests.Infrastructure;

internal sealed class TestAuthenticationHandler(
  IOptionsMonitor<AuthenticationSchemeOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder)
  : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
  internal const string HeaderName = "X-Test-Auth";
  internal const string SchemeName = "Test";
  internal const string InvalidSubMode = "invalid-sub";
  internal const string UserMode = "user";

  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    if (!Request.Headers.TryGetValue(HeaderName, out var modeHeader))
    {
      return Task.FromResult(AuthenticateResult.NoResult());
    }

    var mode = modeHeader.ToString();
    List<Claim> claims;
    if (string.Equals(mode, InvalidSubMode, StringComparison.Ordinal))
    {
      claims =
      [
        new Claim("sub", "not-a-guid"),
        new Claim(ClaimTypes.Email, "test@example.com"),
      ];
    }
    else if (string.Equals(mode, UserMode, StringComparison.Ordinal))
    {
      claims =
      [
        new Claim("sub", "65f87cb7-a030-46f8-af17-a5cd7ae39318"),
        new Claim(ClaimTypes.Email, "test@example.com"),
      ];
    }
    else
    {
      return Task.FromResult(AuthenticateResult.Fail("Unsupported test auth mode."));
    }

    var identity = new ClaimsIdentity(claims, SchemeName);
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, SchemeName);

    return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}
