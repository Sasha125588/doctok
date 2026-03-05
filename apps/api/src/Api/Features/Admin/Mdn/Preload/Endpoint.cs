using Api.Extensions;

namespace Api.Features.Admin.Mdn.Preload;

public sealed class Endpoint : IAdminEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/mdn/preload", async (
            PreloadMdnRequest req,
            Handler handler,
            CancellationToken ct) =>
        {
            var res = await handler.Handle(
                new Command(req.Lang, req.Count, req.Seed, req.Prefix),
                ct);

            return Results.Ok(res);
        })
        .WithName("AdminMdnPreload")
        .WithSummary("Enqueues batch MDN fetch_raw jobs (dev/admin)")
        .Produces<PreloadMdnResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
