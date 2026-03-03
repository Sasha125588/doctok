using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Topics;

public sealed class TopicReadRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<TopicPostRow>> GetPosts(
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
                         order by p.position
                         """;

    using var db = dbf.Create();

    return (await db.QueryAsync<TopicPostRow>(
      new CommandDefinition(query, new { slug, lang, userId }, cancellationToken: ct))).ToList();
  }
}

public sealed record TopicPostRow(
  long Id,
  string Kind,
  string Title,
  string Body,
  int Position,
  int Like_Count,
  int Dislike_Count,
  int Comment_Count,
  string Topic_Slug,
  string Topic_Title,
  string My_Vote,
  double? Popularity
);

// public sealed record FeedItem(
//   long Id,
//   string Title,
//   string TopicSlug,
//   string TopicTitle,
//   string Kind,
//   string Body,
//   int Position,
//   int LikeCount,
//   int DislikeCount,
//   int CommentCount,
//   string MyVote, // "like" | "dislike" | "none",
//   double? Popularity
// );