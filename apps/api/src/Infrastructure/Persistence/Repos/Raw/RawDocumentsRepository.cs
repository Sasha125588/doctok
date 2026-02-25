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
        string? pageType,
        double? popularity,
        DateTimeOffset? sourceModifiedAt,
        string[]? otherLocales,
        CancellationToken ct)
    {
        const string query = """
                             insert into public.raw_documents(source_id, lang, external_ref, title, content, page_type, popularity, source_modified_at, other_locales)
                             values (@sourceId, @lang, @externalRef, @title, @content, @pageType, @popularity, @sourceModifiedAt, @otherLocales)
                             on conflict (source_id, lang, external_ref) do update
                               set title = excluded.title,
                                   content = excluded.content,
                                   page_type = excluded.page_type,
                                   popularity = excluded.popularity,
                                   source_modified_at = excluded.source_modified_at,
                                   other_locales = excluded.other_locales,
                                   fetched_at = now()
                             returning id
                             """;

        using var db = dbf.Create();
        return await db.ExecuteScalarAsync<long>(
            new CommandDefinition(
                query,
                new { sourceId, lang, externalRef, title, content, pageType, popularity, sourceModifiedAt, otherLocales },
                cancellationToken: ct));
    }

    public async Task<RawDocumentForCards?> GetForCardGeneration(
        long sourceId,
        string lang,
        string externalRef,
        CancellationToken ct)
    {
        const string query = """
                             select rd.id, rd.content, td.topic_id
                             from raw_documents rd
                             join topic_documents td on td.raw_document_id = rd.id
                             where rd.source_id = @sourceId
                               and rd.lang = @lang
                               and rd.external_ref = @externalRef
                             """;

        using var db = dbf.Create();
        return await db.QuerySingleOrDefaultAsync<RawDocumentForCards>(
            new CommandDefinition(query, new { sourceId, lang, externalRef }, cancellationToken: ct));
    }

    public sealed record RawDocumentForCards(long Id, string Content, long Topic_Id);
}
