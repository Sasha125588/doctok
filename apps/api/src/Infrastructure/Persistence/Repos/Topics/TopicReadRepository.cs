using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Topics;

public sealed class TopicReadRepository(IDbConnectionFactory dbf)
{
    public async Task<IReadOnlyList<TopicCardRow>> GetCards(
        string slug,
        string lang,
        CancellationToken ct)
    {
      const string query = """
                           select
                             c.id,
                             c.kind,
                             c.body,
                             c.position
                           from topics t
                           join cards c on c.topic_id = t.id
                           where t.slug = @slug
                             and c.lang = @lang
                           order by c.position
                           """;

      using var db = dbf.Create();

      return (await db.QueryAsync<TopicCardRow>(new CommandDefinition(query, new { slug, lang }, cancellationToken: ct))).ToList();
    }

    public sealed record TopicCardRow(
        long Id,
        string Kind,
        string Body,
        int Position);
}
