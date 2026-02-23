using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics;

public static class TopicsEndpoint
{
    public static IEndpointRouteBuilder MapTopics(this IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/{slug}", async (
            string slug,
            string? lang,
            TopicReadRepository topicRepo,
            CancellationToken ct) =>
        {
            var resolvedLang = lang ?? "en";
            var cards = await topicRepo.GetCards(slug, resolvedLang, ct);

            if (cards.Count == 0)
            {
                return Results.NotFound();
            }

            return Results.Ok(cards.Select(c => new Response(
                c.Id,
                c.Kind,
                c.Body,
                c.Position)));
        });

        return app;
    }
}
