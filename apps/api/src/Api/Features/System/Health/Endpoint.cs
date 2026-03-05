using Api.Extensions;

namespace Api.Features.System.Health;

public sealed class Endpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new SystemHealthResponse(true)))
            .AllowAnonymous()
            .WithTags("System")
            .WithSummary("Health check")
            .WithName("SystemHealth")
            .Produces<SystemHealthResponse>(StatusCodes.Status200OK);
    }
}
