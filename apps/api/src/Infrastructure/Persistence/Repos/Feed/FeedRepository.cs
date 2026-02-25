using Dapper;
using Domain.Common;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Feed;

public sealed class FeedRepository(IDbConnectionFactory dbf)
{
    public async Task<IReadOnlyList<FeedRow>> GetPage(
        FeedCursor? cursor,
        string lang,
        int limit,
        CancellationToken ct)
    {
        const string sql = """
                           select
                             c.id,
                             c.kind,
                             c.body,
                             c.position,
                             t.slug as topic_slug,
                             t.title as topic_title,
                             rd.popularity
                           from cards c
                           join topics t on t.id = c.topic_id
                           join raw_documents rd on rd.id = c.raw_document_id
                           where c.lang = @lang
                             and (
                               @cursorPopularity is null
                               or (rd.popularity < @cursorPopularity)
                               or (rd.popularity = @cursorPopularity and c.id < @cursorId)
                               or (rd.popularity is null and @cursorPopularity is null and c.id < @cursorId)
                             )
                           order by rd.popularity desc nulls last, c.id desc
                           limit @limit
                           """;

        using var db = dbf.Create();

        var parameters = new
        {
            cursorPopularity = cursor?.Popularity,
            cursorId = cursor?.Id,
            lang,
            limit,
        };

        return (await db.QueryAsync<FeedRow>(new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();
    }

    public sealed record FeedRow(
        long Id,
        string Kind,
        string Body,
        int Position,
        string Topic_Slug,
        string Topic_Title,
        double? Popularity);
}
