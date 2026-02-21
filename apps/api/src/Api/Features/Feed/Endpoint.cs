using Domain.Common;
using Infrastructure.Persistence.Repos.Feed;

namespace Api.Features.Feed;

public static class FeedEndpoint
{
    public static IEndpointRouteBuilder MapFeed(this IEndpointRouteBuilder app)
    {
        app.MapGet("/feed", async (
            string? cursor,
            string lang,
            int? limit,
            FeedRepository feedRepo,
            CancellationToken ct) =>
        {
            var take = Math.Clamp(limit ?? 20, 1, 50);
            var cursorId = CursorCodec.Decode(cursor);

            var rows = await feedRepo.GetPage(cursorId, lang, take, ct);
            var items = rows.Select(r => new
            {
                cardId = r.Id,
                topicSlug = r.Topic_Slug,
                topicTitle = r.Topic_Title,
                kind = r.Kind,
                body = r.Body,
                position = r.Position
            }).ToList();

            var nextCursor = rows.Count == take
                ? CursorCodec.Encode(rows.Last().Id)
                : null;

            return Results.Ok(new { items, nextCursor });
        });

        return app;
    }
}
