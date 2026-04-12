using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;

namespace Api.Endpoints.Topics;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/topics", async (
      [AsParameters] TopicsGetPostsQueryParams query,
      ClaimsPrincipal user,
      Handler handler,
      CancellationToken ct) =>
    {
      var userId = CurrentUser.GetUserIdOrNull(user);

      var q = new Query(query.Slug ?? string.Empty, query.Lang, userId);
      var result = await handler.Handle(q, ct);

      return result.ToResponse(Results.Ok);
    })
    .WithTags("Topics")
    .WithSummary("Returns posts for a topic")
    .WithName("TopicsGetPosts")
    .Produces<TopicPostsResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }
}
