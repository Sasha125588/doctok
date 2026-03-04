using System.Security.Claims;
using Api.Auth;
using Api.Extensions;

namespace Api.Features.Topics;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/topics/{slug}", async (
      string slug,
      string? lang,
      ClaimsPrincipal user,
      Handler handler,
      CancellationToken ct) =>
    {
      Guid? userId = null;
      if (user.Identity?.IsAuthenticated == true)
        userId = CurrentUser.GetUserIdOrThrow(user);

      var result = await handler.Handle(new Query(slug, lang, userId), ct);
      return result is null ? Results.NotFound() : Results.Ok(result);
    })
    .WithTags("Topics")
    .WithSummary("Returns posts for a topic")
    .WithName("TopicsGetPosts")
    .Produces<TopicPostsResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
  }
}
