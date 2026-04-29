using Api.Extensions;
using Domain.Posts;
using Domain.Shared;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Feed;

public sealed class Handler(FeedRepository feedRepo) : IHandler
{
    public async Task<FeedResponse> Handle(Query query, CancellationToken ct)
    {
        var take = Math.Clamp(query.Limit ?? 20, 1, 50);
        var lang = LanguageHelpers.NormalizeLang(query.Lang);
        var variant = NormalizeVariant(query.Variant);
        var cursor = CursorCodec.Decode<FeedCursor>(query.Cursor);

        var page = await feedRepo.GetPage(cursor, query.UserId, lang, variant, take + 1, ct);

        var pageResult = CursorPage.From(page, take, ToCursor);

        return new FeedResponse(pageResult.Items, pageResult.NextCursor);
    }

    private static string NormalizeVariant(string? variant)
        => string.IsNullOrWhiteSpace(variant)
            ? "original"
            : variant.Trim().ToLowerInvariant();

    private static FeedCursor ToCursor(TopicPostView item)
      => new(item.Popularity, item.Id, item.CreatedAt);
}
