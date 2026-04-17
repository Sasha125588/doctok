using Dapper;
using Domain.Posts;
using Domain.Shared;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class TopicsRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<TopicPostView>> GetPosts(
    TopicPostsCursor? cursor,
    string slug,
    string lang,
    Guid? userId,
    int limit,
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
                           v.value as my_vote,
                           rd.popularity,
                           p.created_at
                         from topics t
                         join posts p on p.topic_id = t.id
                         join raw_documents rd on rd.id = p.raw_document_id
                         left join post_reactions v
                            on v.post_id = p.id
                            and v.user_id = @userId
                         where t.slug = @slug
                           and p.lang = @lang
                           and (
                           @cursorId is null 
                             or (
                               case p.kind
                                 when 'summary' then 0
                                 when 'concept' then 1
                                 when 'example' then 2
                                 when 'tip' then 3
                                 else 4
                               end,
                               p.position,
                               p.id
                             ) > (@cursorKindRank, @cursorPosition, @cursorId)
                           )
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
                         limit @limit
                         """;

    using var db = dbf.Create();

    var parameters = new
    {
      cursorKindRank = cursor?.KindRank,
      cursorPosition = cursor?.Position,
      cursorId = cursor?.Id,
      slug,
      lang,
      userId,
      limit,
    };

    return (await db.QueryAsync<TopicPostView>(
      new CommandDefinition(query, parameters, cancellationToken: ct))).ToList();
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

  public async Task<long> EnsureTopic(string slug, string title, CancellationToken ct)
  {
      const string sql = """
                         insert into public.topics(slug, title)
                         values (@slug, @title)
                         on conflict (slug) do update
                           set title = excluded.title
                         returning id
                         """;

      using var db = dbf.Create();
      return await db.ExecuteScalarAsync<long>(
          new CommandDefinition(sql, new { slug, title }, cancellationToken: ct));
  }
}
