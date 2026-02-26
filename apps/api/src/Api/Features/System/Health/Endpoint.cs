using Api.Extensions;

namespace Api.Features.System.Health;

public sealed class HealthEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { ok = true }))
            .AllowAnonymous()
            .WithTags("System")
            .WithSummary("Health check")
            .Produces(StatusCodes.Status200OK);
    }
}
