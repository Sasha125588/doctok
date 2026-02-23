using Domain.Common;
using Infrastructure.Persistence.Repos.Feed;

namespace Api.Features.Feed;

public sealed record FeedItem(
    long CardId,
    string TopicSlug,
    string TopicTitle,
    string Kind,
    string Body,
    int Position);

public static class FeedEndpoint
{
    public static IEndpointRouteBuilder MapFeed(this IEndpointRouteBuilder app)
    {
        app.MapGet("/feed", async (
            string? cursor,
            string? lang,
            int? limit,
            FeedRepository feedRepo,
            CancellationToken ct) =>
        {
            var take = Math.Clamp(limit ?? 20, 1, 50);
            var resolvedLang = lang ?? "en";
            var cursorId = CursorCodec.Decode(cursor);

            var rows = await feedRepo.GetPage(cursorId, resolvedLang, take, ct);
            var items = rows.Select(r => new FeedItem(
                r.Id,
                r.Topic_Slug,
                r.Topic_Title,
                r.Kind,
                r.Body,
                r.Position)).ToList();

            var nextCursor = rows.Count == take
                ? CursorCodec.Encode(rows.Last().Id)
                : null;

            return Results.Ok(new Response(items, nextCursor));
        });

        return app;
    }
}
