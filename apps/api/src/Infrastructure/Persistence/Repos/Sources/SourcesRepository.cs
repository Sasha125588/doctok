using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Sources;

public sealed class SourcesRepository(IDbConnectionFactory dbf)
{
    public async Task<long> GetSourceIdByCode(string code, CancellationToken ct = default)
    {
      const string query = @"
            select id
            from public.sources
            where code = @code
            ";

      var db = dbf.Create();
      var id = await db.ExecuteScalarAsync<long?>(
          new CommandDefinition(query, new { code }, cancellationToken: ct));

      return id ?? throw new InvalidOperationException($"Unknown source code: {code}");
    }
}
