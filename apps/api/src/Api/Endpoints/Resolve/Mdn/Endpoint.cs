using Api.Errors;
using Api.Extensions;

namespace Api.Endpoints.Resolve.Mdn;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/resolve/mdn", async (
        [AsParameters] ResolveMdnQueryParams query,
        Handler handler,
        CancellationToken ct) =>
      {
        var q = new Query(query.ExternalRef ?? string.Empty, query.Lang ?? "en");
        var result = await handler.Handle(q, ct);

        return result.ToResponse(Results.Ok);
      })
      .WithTags("Resolve")
      .WithSummary("Resolves an MDN external path to a DocTok topic")
      .WithName("ResolveMdn")
      .Produces<ResolveMdnResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }
}
