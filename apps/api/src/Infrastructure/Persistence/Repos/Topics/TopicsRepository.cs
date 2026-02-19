using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Topics;

public sealed class TopicsRepository(IDbConnectionFactory dbf)
{
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