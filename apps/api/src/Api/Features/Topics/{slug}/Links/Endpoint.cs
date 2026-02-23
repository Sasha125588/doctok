using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics._slug_.Links;

public static class TopicsLinksEndpoint
{
    public static IEndpointRouteBuilder MapTopicsLinks(this IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/{slug}/links", async (
            string slug,
            string? lang,
            TopicLinksRepository topicLinksRepo,
            CancellationToken ct) =>
        {
            var resolvedLang = lang ?? "en";
            var links = await topicLinksRepo.GetLinkedTopics(slug, resolvedLang, ct);
            return Results.Ok(links);
        });

        return app;
    }
}
