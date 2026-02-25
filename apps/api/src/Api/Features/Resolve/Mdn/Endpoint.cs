namespace Api.Features.Resolve.Mdn;

public static class ResolveMdnEndpoint
{
    public static IEndpointRouteBuilder MapResolveMdn(this IEndpointRouteBuilder app)
    {
        app.MapGet("/resolve/mdn/{*externalRef}", async (
            string externalRef,
            string? lang,
            ResolveMdnHandler handler,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(externalRef))
            {
                return Results.BadRequest(new { error = "externalRef is required" });
            }

            var q = new Query(externalRef, lang ?? "en");
            var res = await handler.Handle(q, ct);

            return Results.Ok(res);
        });

        return app;
    }
}
