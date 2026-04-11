using Dapper;
using Domain.Common;
using Domain.Models;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Feed;

public sealed class FeedRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<PostItem>> GetPage(
    FeedCursor? cursor,
    Guid? userId,
    string lang,
    int limit,
    CancellationToken ct)
  {
    const string sql = """
                       select
                         p.id,
                         p.kind,
                         p.title,
                         p.body,
                         p.position,
                         p.like_count,
                         p.dislike_count,
                         p.comment_count,
                         t.slug as topic_slug,
                         t.title as topic_title,
                         coalesce(v.value::text, 'none') as my_vote,
                         rd.popularity
                       from posts p
                       join topics t on t.id = p.topic_id
                       join raw_documents rd on rd.id = p.raw_document_id
                       left join post_reactions v
                         on v.post_id = p.id
                        and v.user_id = @userId
                       where p.lang = @lang
                         and (
                           @cursorPopularity is null
                           or (rd.popularity < @cursorPopularity)
                           or (rd.popularity = @cursorPopularity and p.id < @cursorId)
                           or (rd.popularity is null and @cursorPopularity is null and p.id < @cursorId)
                         )
                       order by rd.popularity desc nulls last, p.id desc
                       limit @limit
                       """;

    using var db = dbf.Create();

    var parameters = new
    {
      cursorPopularity = cursor?.Popularity,
      cursorId = cursor?.Id,
      userId,
      lang,
      limit,
    };

    return (await db.QueryAsync<PostItem>(
      new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();
  }
}
