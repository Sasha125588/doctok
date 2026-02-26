using Api.Extensions;

namespace Api.Features.Admin.Mdn.Preload;

public sealed class Endpoint : IAdminEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/mdn/preload", async (
            Request req,
            PreloadMdnHandler handler,
            CancellationToken ct) =>
        {
            var res = await handler.Handle(
                new Command(req.Lang, req.Count, req.Seed, req.Prefix),
                ct);

            return Results.Ok(new Response(res.Sample));
        })
        .WithSummary("Enqueues batch MDN fetch_raw jobs (dev/admin)")
        .Produces<Response>(StatusCodes.Status200OK);
    }
}
