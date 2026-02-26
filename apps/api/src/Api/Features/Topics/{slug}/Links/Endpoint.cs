using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics._slug_.Links;

public sealed class TopicsLinksEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/{slug}/links", async (
            string slug,
            string? lang,
            TopicLinksRepository topicLinksRepo,
            CancellationToken ct) =>
        {
            var resolvedLang = LanguageHelpers.NormalizeLang(lang ?? "en");
            var links = await topicLinksRepo.GetLinkedTopics(slug, resolvedLang, ct);
            return Results.Ok(links);
        });
    }
}
