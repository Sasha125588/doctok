using Dapper;
using Domain.Models;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Topics;

public sealed class TopicReadRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<PostItem>> GetPosts(
    string slug,
    string lang,
    Guid? userId,
    CancellationToken ct)
  {
    const string query = """
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
                         from topics t
                         join posts p on p.topic_id = t.id
                         join raw_documents rd on rd.id = p.raw_document_id
                         left join post_votes v
                            on v.post_id = p.id
                            and v.user_id = @userId
                         where t.slug = @slug
                           and p.lang = @lang
                         order by
                           case p.kind
                             when 'summary' then 0
                             when 'example' then 1
                             when 'fact' then 2
                             else 3
                           end,
                           p.position,
                           p.id
                         """;

    using var db = dbf.Create();

    return (await db.QueryAsync<PostItem>(
      new CommandDefinition(query, new { slug, lang, userId }, cancellationToken: ct))).ToList();
  }
}
