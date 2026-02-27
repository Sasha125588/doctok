using Dapper;
using Domain.Common;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Feed;

public sealed class FeedRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<FeedRow>> GetPage(
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
                         p.body,
                         p.position,
                         p.like_count,
                         p.dislike_count,
                         p.comment_count,
                         t.slug as topic_slug,
                         t.title as topic_title,
                         rd.popularity,
                         coalesce(v.value::text, 'none') as my_vote
                       from posts p
                       join topics t on t.id = p.topic_id
                       join raw_documents rd on rd.id = p.raw_document_id
                       left join post_votes v
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
      userId, // null => join не матчиться => 'none'
      lang,
      limit,
    };

    return (await db.QueryAsync<FeedRow>(
      new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();
  }

  public sealed record FeedRow(
    long Id,
    string Kind,
    string Body,
    int Position,
    int Like_Count,
    int Dislike_Count,
    int Comment_Count,
    string Topic_Slug,
    string Topic_Title,
    double? Popularity,
    string My_Vote
  );
}
