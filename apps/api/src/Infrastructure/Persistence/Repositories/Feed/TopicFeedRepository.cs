using Dapper;
using Domain.Common;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class TopicFeedRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<TopicFeedPageItem>> GetPage(
    FeedCursor? cursor,
    string lang,
    int limit,
    CancellationToken ct)
  {
    /* language=postgresql */
    const string sql = """
                       with topic_candidates as (
                         select
                           t.id,
                           t.slug,
                           t.title,
                           max(rd.popularity) as popularity
                         from topics t
                         join posts p
                           on p.topic_id = t.id
                          and p.lang = @lang
                         join raw_documents rd on rd.id = p.raw_document_id
                         group by t.id, t.slug, t.title
                       ),
                       paged_topics as (
                         select
                           tc.id,
                           tc.slug,
                           tc.title,
                           tc.popularity
                         from topic_candidates tc
                         where @cursorId is null
                            or (
                              coalesce(tc.popularity, '-infinity'::double precision) < coalesce(@cursorPopularity, '-infinity'::double precision)
                              or (
                                coalesce(tc.popularity, '-infinity'::double precision) = coalesce(@cursorPopularity, '-infinity'::double precision)
                                and tc.id < @cursorId
                              )
                            )
                         order by tc.popularity desc nulls last, tc.id desc
                         limit @limit
                       )
                       select
                         pt.id,
                         pt.slug,
                         pt.title,
                         @lang as lang,
                         counts.post_count,
                         preview.preview_post_id,
                         preview.preview_kind,
                         preview.preview_title,
                         preview.preview_body,
                         preview.preview_body_html,
                         pt.popularity
                       from paged_topics pt
                       join lateral (
                         select count(*)::int as post_count
                         from posts p
                         where p.topic_id = pt.id
                           and p.lang = @lang
                       ) counts on true
                       join lateral (
                         select
                           p.id as preview_post_id,
                           p.kind as preview_kind,
                           p.title as preview_title,
                           p.body as preview_body,
                           p.body_html as preview_body_html
                         from posts p
                         where p.topic_id = pt.id
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
                         limit 1
                       ) preview on true
                       order by pt.popularity desc nulls last, pt.id desc
                       """;

    using var db = dbf.Create();

    var parameters = new
    {
      cursorId = cursor?.Id,
      cursorPopularity = cursor?.Popularity,
      lang,
      limit,
    };

    return (await db.QueryAsync<TopicFeedPageItem>(
      new CommandDefinition(sql, parameters, cancellationToken: ct))).ToList();
  }
}

public sealed record TopicFeedPageItem(
  long Id,
  string Slug,
  string Title,
  string Lang,
  int PostCount,
  long PreviewPostId,
  string PreviewKind,
  string? PreviewTitle,
  string PreviewBody,
  string PreviewBodyHtml,
  double? Popularity);
