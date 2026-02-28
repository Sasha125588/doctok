using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Topics;

public sealed class TopicReadRepository(IDbConnectionFactory dbf)
{
    public async Task<IReadOnlyList<TopicPostRow>> GetPosts(
        string slug,
        string lang,
        CancellationToken ct)
    {
      const string query = """
                           select
                             p.id,
                             p.title,
                             p.kind,
                             p.body,
                             p.position,
                             p.like_count,
                             p.dislike_count,
                             p.comment_count
                           from topics t
                           join posts p on p.topic_id = t.id
                           where t.slug = @slug
                             and p.lang = @lang
                           order by p.position
                           """;

      using var db = dbf.Create();

      return (await db.QueryAsync<TopicPostRow>(new CommandDefinition(query, new { slug, lang }, cancellationToken: ct))).ToList();
    }

    public sealed record TopicPostRow(
        long Id,
        string Title,
        string Kind,
        string Body,
        int Position,
        int Like_Count,
        int Dislike_Count,
        int Comment_Count);
}
