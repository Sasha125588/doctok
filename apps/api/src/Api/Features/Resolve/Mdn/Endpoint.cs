namespace Api.Features.Resolve.Mdn;

public static class ResolveMdnEndpoint
{
    public static IEndpointRouteBuilder MapResolveMdn(this IEndpointRouteBuilder app)
    {
        app.MapGet("/resolve/mdn/{*externalRef}", async (
            string externalRef,
            string? lang,
            Handler handler,
            CancellationToken ct) =>
        {
            var q = new Query(externalRef, lang ?? "en");
            var res = await handler.Handle(q, ct);

            return Results.Ok(res);
        });

        return app;
    }
}