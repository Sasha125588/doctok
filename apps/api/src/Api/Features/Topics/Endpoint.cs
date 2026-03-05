using System.Security.Claims;
using Api.Auth;
using Api.Errors;
using Api.Extensions;

namespace Api.Features.Topics;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/topics/{slug}", async (
      string slug,
      [AsParameters] TopicsGetPostsQueryParams query,
      ClaimsPrincipal user,
      Handler handler,
      CancellationToken ct) =>
    {
      Guid? userId = null;
      if (user.Identity?.IsAuthenticated == true)
        userId = CurrentUser.GetUserIdOrThrow(user);

      var result = await handler.Handle(new Query(slug, query.Lang, userId), ct);
      return result.ToResponse(value => Results.Ok(value));
    })
    .WithTags("Topics")
    .WithSummary("Returns posts for a topic")
    .WithName("TopicsGetPosts")
    .Produces<TopicPostsResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }
}
