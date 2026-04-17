using Dapper;
using Domain.Shared;
using Domain.Topics;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class TopicFeedRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<TopicFeedPageView>> GetPage(
    FeedCursor? cursor,
    string lang,
    int limit,
    CancellationToken ct)
  {
    const string sql = """
                       select
                         tc.id,
                         tc.slug,
                         tc.title,
                         tc.post_count,
                         tc.popularity,
                         tc.created_at
                       from (
                         select
                           t.id,
                           t.slug,
                           t.title,
                           count(*)::int as post_count,
                           max(rd.popularity) as popularity,
                           t.created_at
                         from topics t
                         join posts p
                           on p.topic_id = t.id
                          and p.lang = @lang
                         join raw_documents rd
                           on rd.id = p.raw_document_id
                         group by t.id, t.slug, t.title
                       ) tc
                       where (
                        @cursorId is null 
                        or (tc.popularity, tc.id, tc.created_at) 
                             < (@cursorPopularity, @cursorId, @cursorCreatedAt)
                       )
                       order by
                         tc.popularity desc,
                         tc.id desc,
                         tc.created_at desc
                       limit @limit;
                       """;

    using var db = dbf.Create();

    var parameters = new
    {
      cursorId = cursor?.Id,
      cursorPopularity = cursor?.Popularity,
      cursorCreatedAt = cursor?.CreatedAt,
      lang,
      limit,
    };

    return (await db.QueryAsync<TopicFeedPageView>(
      new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();
  }
}
