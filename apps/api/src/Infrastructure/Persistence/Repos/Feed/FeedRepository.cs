using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Feed;

public sealed class FeedRepository(IDbConnectionFactory dbf)
{
    public async Task<IReadOnlyList<FeedRow>> GetPage(
        long? cursorId,
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
                           t.title as topic_title
                         from cards c
                         join topics t on t.id = c.topic_id
                         where (@cursorId is null or c.id < @cursorId)
                           and c.lang = @lang
                         order by c.id desc
                         limit @limit
                         """;

      using var db = dbf.Create();

      return (await db.QueryAsync<FeedRow>(new CommandDefinition(sql, new { cursorId, lang, limit }, cancellationToken: ct))).ToList();
    }

    public sealed record FeedRow(
        long Id,
        string Kind,
        string Body,
        int Position,
        string Topic_Slug,
        string Topic_Title);
}
