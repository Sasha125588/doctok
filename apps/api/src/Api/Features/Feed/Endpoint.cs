using System.Security.Claims;
using Api.Auth;
using Api.Extensions;

namespace Api.Features.Feed;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/feed", async (
        string? cursor,
        string? lang,
        int? limit,
        ClaimsPrincipal user,
        Handler handler,
        CancellationToken ct) =>
      {
        Guid? userId = null;
        if (user.Identity?.IsAuthenticated == true)
          userId = CurrentUser.GetUserIdOrThrow(user);

        var result = await handler.Handle(new Query(cursor, lang, limit, userId), ct);
        return Results.Ok(result);
      })
      .WithTags("Feed")
      .WithSummary("Returns paginated feed items")
      .WithName("FeedList")
      .Produces<FeedResponse>(StatusCodes.Status200OK);
  }
}
