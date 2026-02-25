namespace Api.Features.Admin.Mdn.Preload;

public static class Endpoint
{
  public static IEndpointRouteBuilder MapAdminMdnPreload(this IEndpointRouteBuilder app)
  {
    app.MapPost("/api/admin/mdn/preload", async (
        Request req,
        PreloadMdnHandler handler,
        CancellationToken ct) =>
      {
        var res = await handler.Handle(
          new Command(req.Lang, req.Count, req.Seed, req.Prefix),
          ct);

        return Results.Ok(new Response(res.Sample));
      })
      .RequireAuthorization("Admin")
      .WithTags("Admin")
      .WithSummary("Enqueues batch MDN fetch_raw jobs (dev/admin)")
      .Produces<Response>(StatusCodes.Status200OK);

    return app;
  }
}
