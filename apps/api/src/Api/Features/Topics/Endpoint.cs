using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics;

public sealed class TopicsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/topics/{slug}", async (
            string slug,
            string? lang,
            TopicReadRepository topicRepo,
            CancellationToken ct) =>
        {
            var resolvedLang = LanguageHelpers.NormalizeLang(lang ?? "en");
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
    }
}
