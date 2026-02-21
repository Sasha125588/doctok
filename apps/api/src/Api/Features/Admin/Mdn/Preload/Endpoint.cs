namespace Api.Features.Admin.Mdn.Preload;

public static class PreloadMdnEndpoint
{
  public sealed record Request(string Lang, int Count, int? Seed, string? Prefix);

  public static IEndpointRouteBuilder MapAdminMdnPreload(this IEndpointRouteBuilder app)
  {
    app.MapPost("/api/admin/mdn/preload", async (
        Request req,
        PreloadMdnHandler handler,
        CancellationToken ct) =>
      {
        var res = await handler.Handle(
          new PreloadMdnCommand(req.Lang, req.Count, req.Seed, req.Prefix),
          ct);

        return Results.Ok(new { enqueued = res.Enqueued, sample = res.Sample.Take(10) });
      })
      .RequireAuthorization()
      .WithTags("Admin")
      .WithSummary("Enqueues batch MDN fetch_raw jobs (dev/admin)")
      .Produces(StatusCodes.Status200OK);

    return app;
  }
}
