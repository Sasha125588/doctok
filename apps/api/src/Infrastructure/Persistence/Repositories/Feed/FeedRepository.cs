using Dapper;
using Domain.Posts;
using Domain.Shared;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class FeedRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<TopicPostView>> GetPage(
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
                         p.body_html,
                         p.position,
                         p.like_count,
                         p.dislike_count,
                         p.comment_count,
                         t.slug as topic_slug,
                         t.title as topic_title,
                         v.value as my_vote,
                         rd.popularity,
                         p.created_at
                       from posts p
                       join topics t on t.id = p.topic_id
                       join raw_documents rd on rd.id = p.raw_document_id
                       left join post_reactions v
                         on v.post_id = p.id
                        and v.user_id = @userId
                       where p.lang = @lang 
                         and (
                           @cursorId is null 
                             or (rd.popularity, p.created_at, p.id) 
                                < (@cursorPopularity, @cursorCreatedAt, @cursorId)
                           )
                       order by rd.popularity desc nulls last, p.created_at desc, p.id desc
                       limit @limit
                       """;

    using var db = dbf.Create();

    var parameters = new
    {
      cursorPopularity = cursor?.Popularity,
      cursorId = cursor?.Id,
      cursorCreatedAt = cursor?.CreatedAt,
      userId,
      lang,
      limit,
    };

    return (await db.QueryAsync<TopicPostView>(
      new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();
  }
}
