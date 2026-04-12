using System.Text.Json;
using Dapper;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class JobsRepository(IDbConnectionFactory dbf)
{
    public async Task<long> Enqueue(string jobType, string jobKey, object payload, CancellationToken ct = default)
    {
        const string sql = """
                           insert into public.jobs(job_type, job_key, payload, status)
                           values (@jobType, @jobKey, cast(@payload as jsonb), 'pending')
                           on conflict (job_key) do update
                             set job_type = excluded.job_type,
                                 payload = excluded.payload,
                                 status = 'pending',
                                 last_error = null,
                                 attempts = 0,
                                 next_attempt_at = now(),
                                 updated_at = now()
                           where public.jobs.status <> 'running'
                           returning id
                           """;

        var payloadJson = JsonSerializer.Serialize(payload);

        using var db = dbf.Create();
        return await db.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, new { jobType, jobKey, payload = payloadJson }, cancellationToken: ct));
    }

    public async Task<JobEnvelope?> Dequeue(CancellationToken ct = default)
    {
        const string sql = """
                           with j as (
                             select id
                             from public.jobs
                             where status = 'pending'
                               and next_attempt_at <= now()
                             order by created_at
                             for update skip locked
                             limit 1
                           )
                           update public.jobs jb
                           set status = 'running',
                               attempts = attempts + 1,
                               updated_at = now()
                           from j
                           where jb.id = j.id
                           returning jb.id as Id,
                                     jb.job_type as JobType,
                                     jb.job_key as JobKey,
                                     jb.payload::text as PayloadJson,
                                     jb.attempts as Attempts
                           """;

        using var db = dbf.Create();

        var row = await db.QuerySingleOrDefaultAsync<DequeueRow>(new CommandDefinition(sql, cancellationToken: ct));

        if (row is null)
            return null;

        return new JobEnvelope(row.Id, row.JobType, row.JobKey, row.PayloadJson, row.Attempts);
    }

    public async Task MarkDone(long jobId, CancellationToken ct = default)
    {
        const string sql = """
                           update public.jobs
                           set status = 'done',
                               last_error = null,
                               updated_at = now()
                           where id = @jobId
                           """;

        using var db = dbf.Create();
        await db.ExecuteAsync(new CommandDefinition(sql, new { jobId }, cancellationToken: ct));
    }

    public async Task MarkFailed(long jobId, string error, CancellationToken ct = default)
    {
        const string sql = """
                           update public.jobs
                           set status = 'failed',
                               last_error = @error,
                               updated_at = now()
                           where id = @jobId
                           """;

        using var db = dbf.Create();
        await db.ExecuteAsync(new CommandDefinition(sql, new { jobId, error }, cancellationToken: ct));
    }

    public async Task MarkPendingForRetry(long jobId, string error, TimeSpan delay, CancellationToken ct = default)
    {
        const string sql = """
                           update public.jobs
                           set status = 'pending',
                               last_error = @error,
                               next_attempt_at = now() + @delay,
                               updated_at = now()
                           where id = @jobId
                           """;

        using var db = dbf.Create();
        await db.ExecuteAsync(new CommandDefinition(sql, new { jobId, error, delay }, cancellationToken: ct));
    }

    public async Task<int> RequeueStaleRunning(TimeSpan staleAfter, CancellationToken ct = default)
    {
        const string sql = """
                           update public.jobs
                           set status = 'pending',
                               last_error = 'recovered stale running job',
                               updated_at = now()
                           where status = 'running'
                             and updated_at < now() - @staleAfter
                           """;

        using var db = dbf.Create();
        return await db.ExecuteAsync(new CommandDefinition(sql, new { staleAfter }, cancellationToken: ct));
    }

    private sealed record DequeueRow(long Id, string JobType, string JobKey, string PayloadJson, int Attempts);
}
