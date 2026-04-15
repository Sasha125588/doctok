using Dapper;
using Domain.Models;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

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
                           p.body_html,
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
                         left join post_reactions v
                            on v.post_id = p.id
                            and v.user_id = @userId
                         where t.slug = @slug
                           and p.lang = @lang
                         order by
                           case p.kind
                             when 'summary' then 0
                             when 'concept' then 1
                             when 'example' then 2
                             when 'tip' then 3
                             else 4
                           end,
                           p.position,
                           p.id
                         """;

    using var db = dbf.Create();

    return (await db.QueryAsync<PostItem>(
      new CommandDefinition(query, new { slug, lang, userId }, cancellationToken: ct))).ToList();
  }

  public async Task<bool> PostsExistForTopic(string slug, string lang, CancellationToken ct)
  {
    const string query = """
                         select exists(
                          select 1
                          from posts p
                          join topics t on t.id = p.topic_id
                          where t.slug = @slug
                          and p.lang = @lang
                         )
                         """;

    using var db = dbf.Create();

    return await db.ExecuteScalarAsync<bool>(new CommandDefinition(query, new { slug, lang }, cancellationToken: ct));
  }
}
