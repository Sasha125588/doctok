using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Raw;

public sealed class RawDocumentsRepository(IDbConnectionFactory dbf)
{
    public async Task<long> UpsertRawDocument(
        long sourceId,
        string lang,
        string externalRef,
        string? title,
        string content,
        CancellationToken ct)
    {
        const string sql = """
                           insert into public.raw_documents(source_id, lang, external_ref, title, content)
                           values (@sourceId, @lang, @externalRef, @title, @content)
                           on conflict (source_id, lang, external_ref) do update
                             set title = excluded.title,
                                 content = excluded.content,
                                 fetched_at = now()
                           returning id
                           """;

        using var db = dbf.Create();
        return await db.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, new { sourceId, lang, externalRef, title, content }, cancellationToken: ct));
    }
}