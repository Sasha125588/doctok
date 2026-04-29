using Api.Errors;
using Api.Extensions;

namespace Api.Endpoints.Posts.Content;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/posts/{postId:long}/content", async (
        long postId,
        [AsParameters] PostsGetContentQueryParams query,
        Handler handler,
        CancellationToken ct) =>
      {
        var q = new Query(postId, query.Variant);
        var result = await handler.Handle(q, ct);

        return result.ToResponse(Results.Ok);
      })
      .WithTags("Posts")
      .WithSummary("Returns content for a post variant")
      .WithName("PostsGetContent")
      .Produces<Domain.Posts.PostContentView>(StatusCodes.Status200OK)
      .ProducesProblem(StatusCodes.Status404NotFound)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }
}
