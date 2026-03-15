using Api.Extensions;
using Domain.Common;
using Domain.Models;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics._slug_.Links;

public sealed class Endpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/links", async (
            [AsParameters] TopicsGetLinksQueryParams query,
            TopicLinksRepository topicLinksRepo,
            CancellationToken ct) =>
        {
            var slug = (query.Slug ?? string.Empty).Trim().Trim('/');
            var resolvedLang = LanguageHelpers.NormalizeLang(query.Lang ?? "en");
            var links = await topicLinksRepo.GetLinkedTopics(slug, resolvedLang, ct);
            return Results.Ok(links);
        })
        .WithTags("Topics")
        .WithSummary("Returns linked topics for a topic")
        .WithName("TopicsGetLinks")
        .Produces<IReadOnlyList<TopicLink>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }
}
