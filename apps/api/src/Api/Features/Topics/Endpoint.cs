using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics;

public static class TopicsEndpoint
{
    public static IEndpointRouteBuilder MapTopics(this IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/{slug}", async (
            string slug,
            string lang,
            TopicReadRepository topicRepo,
            CancellationToken ct) =>
        {
            var cards = await topicRepo.GetCards(slug, lang, ct);

            if (cards.Count == 0)
            {
                return Results.NotFound();
            }

            return Results.Ok(cards.Select(c => new
            {
                id = c.Id,
                kind = c.Kind,
                body = c.Body,
                position = c.Position
            }));
        });

        return app;
    }

    public static IEndpointRouteBuilder MapTopicsLinks(this IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/{slug}/links", async (
            string slug,
            string lang,
            TopicLinksRepository topicLinksRepo,
            CancellationToken ct) =>
        {
            var links = await topicLinksRepo.GetLinkedTopics(slug, lang, ct);
            return Results.Ok(links);
        });

        return app;
    }
}
