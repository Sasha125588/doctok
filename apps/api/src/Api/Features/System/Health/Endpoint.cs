namespace Api.Features.System.Health;

public static class HealthEndpoint
{
    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { ok = true }))       
            .AllowAnonymous()
            .WithTags("System")
            .WithSummary("Health check")
            .Produces(StatusCodes.Status200OK);

        return app;
    }
}
