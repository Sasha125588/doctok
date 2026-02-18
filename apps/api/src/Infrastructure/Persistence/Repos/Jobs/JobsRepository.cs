using System.Text.Json;
using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Jobs;

public sealed class JobsRepository(IDbConnectionFactory dbf)
{
    public async Task<long> Enqueue(string jobType, string jobKey, object payload, CancellationToken ct = default)
    {
        const string sql = """
                           insert into public.jobs(job_type, job_key, payload, status)
                           values (@jobType, @jobKey, cast(@payload as jsonb), 'pending')
                           on conflict (job_key) do update
                             set updated_at = now()
                           returning id
                           """;

        var payloadJson = JsonSerializer.Serialize(payload);

        using var db = dbf.Create();
        return await db.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, new { jobType, jobKey, payload = payloadJson }, cancellationToken: ct));
    }
}
