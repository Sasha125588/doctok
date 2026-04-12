using Api.Extensions;

namespace Api.Endpoints.Feed.Topics;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/feed/topics", async (
        [AsParameters] FeedTopicsListQueryParams query,
        Handler handler,
        CancellationToken ct) =>
      {
        var result = await handler.Handle(new Query(query.Cursor, query.Lang, query.Limit), ct);
        return Results.Ok(result);
      })
      .WithTags("Feed")
      .WithSummary("Returns paginated topics for the vertical swipe feed")
      .WithName("FeedTopicsList")
      .Produces<TopicFeedResponse>(StatusCodes.Status200OK)
      .ProducesValidationProblem(StatusCodes.Status400BadRequest);
  }
}
