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
    string variant,
    int limit,
    CancellationToken ct)
  {
    const string sql = """
                       select
                         p.id,
                         p.kind,
                         coalesce(requested_variant.variant_code, original_variant.variant_code) as variant_code,
                         coalesce(requested_variant.title, original_variant.title, p.title) as title,
                         coalesce(requested_variant.body, original_variant.body, p.body) as body,
                         coalesce(requested_variant.body_html, original_variant.body_html, p.body_html) as body_html,
                         p.position,
                         p.like_count,
                         p.dislike_count,
                         p.comment_count,
                         t.slug as topic_slug,
                         t.title as topic_title,
                         v.value as my_vote,
                         rd.popularity,
                         p.created_at,
                         false as is_saved
                       from posts p
                       join topics t on t.id = p.topic_id
                       join raw_documents rd on rd.id = p.raw_document_id
                       left join post_content_variants requested_variant
                         on requested_variant.post_id = p.id
                        and requested_variant.variant_code = @variant
                       left join post_content_variants original_variant
                         on original_variant.post_id = p.id
                        and original_variant.variant_code = 'original'
                       left join post_reactions v
                         on v.post_id = p.id
                        and v.user_id = @userId
                       where p.lang = @lang
                         and p.is_active = true
                         and (
                           @cursorId is null
                             or (rd.popularity, p.created_at, p.id)
                                < (@cursorPopularity, @cursorCreatedAt, @cursorId)
                           )
                       order by rd.popularity desc nulls last, p.created_at desc, p.id desc
                       limit @limit
                       """;

    await using var db = dbf.Create();

    var parameters = new
    {
      cursorPopularity = cursor?.Popularity,
      cursorId = cursor?.Id,
      cursorCreatedAt = cursor?.CreatedAt,
      userId,
      lang,
      variant,
      limit,
    };

    return (await db.QueryAsync<TopicPostView>(
      new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();
  }
}
