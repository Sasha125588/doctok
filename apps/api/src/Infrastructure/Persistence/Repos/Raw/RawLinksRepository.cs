using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Raw;

public sealed class RawLinksRepository(IDbConnectionFactory dbf)
{
    public async Task InsertInternalLinks(
        long rawDocumentId,
        long targetSourceId,
        IEnumerable<(string targetLang, string targetExternalRef, string? label)> links,
        CancellationToken ct)
    {
        const string sql = @"
                           insert into public.raw_document_links(
                             raw_document_id, kind, target_source_id, target_lang, target_external_ref, label
                           )
                           values (@rawDocumentId, 'internal', @targetSourceId, @targetLang, @targetExternalRef, @label)
                           on conflict (raw_document_id, kind, target_source_id, target_lang, target_external_ref) do nothing
                           ";

        using var db = dbf.Create();

        foreach (var (targetLang, targetExternalRef, label) in links)
        {
            await db.ExecuteAsync(new CommandDefinition(
              sql,
              new
              {
                  rawDocumentId,
                  targetSourceId,
                  targetLang,
                  targetExternalRef,
                  label
              }, cancellationToken: ct));
        }
    }

    public async Task InsertExternalLinks(
        long rawDocumentId,
        IEnumerable<(string url, string? label)> links,
        CancellationToken ct)
    {
        const string sql = """
                           insert into public.raw_document_links(
                             raw_document_id, kind, url, label
                           )
                           values (@rawDocumentId, 'external', @url, @label)
                           on conflict (raw_document_id, kind, url) do nothing
                           """;

        using var db = dbf.Create();

        foreach (var (url, label) in links)
        {
            await db.ExecuteAsync(new CommandDefinition(sql, new { rawDocumentId, url, label }, cancellationToken: ct));
        }
    }
}
