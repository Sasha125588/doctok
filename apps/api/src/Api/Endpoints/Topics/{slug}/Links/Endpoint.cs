using Api.Extensions;
using Domain.Shared;
using Domain.Topics;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Topics._slug_.Links;

public sealed class Endpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/links", async (
            [AsParameters] TopicsGetLinksQueryParams query,
            TopicLinksRepository topicLinksRepo,
            CancellationToken ct) =>
        {
            var slug = query.Slug.Trim().Trim('/');
            var resolvedLang = LanguageHelpers.NormalizeLang(query.Lang);
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
