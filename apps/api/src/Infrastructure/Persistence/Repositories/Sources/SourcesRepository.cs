using Dapper;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class SourcesRepository(IDbConnectionFactory dbf)
{
    public async Task<long> GetSourceIdByCode(string code, CancellationToken ct = default)
    {
        const string query = """
                             select id
                             from public.sources
                             where code = @code
                             """;

        using var db = dbf.Create();
        var id = await db.ExecuteScalarAsync<long?>(
            new CommandDefinition(query, new { code }, cancellationToken: ct));

        var result = id ?? throw new InvalidOperationException($"Unknown source code: {code}");
        return result;
    }
}
