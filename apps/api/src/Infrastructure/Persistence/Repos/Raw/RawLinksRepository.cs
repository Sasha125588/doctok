using System.Globalization;
using System.Text;
using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Raw;

public sealed class RawLinksRepository(IDbConnectionFactory dbf)
{
    public async Task InsertInternalLinks(
        long rawDocumentId,
        long targetSourceId,
        IReadOnlyList<(string targetLang, string targetExternalRef, string? label)> links,
        CancellationToken ct)
    {
        if (links.Count == 0)
            return;

        var sb = new StringBuilder();
        sb.Append("""
                  insert into public.raw_document_links(
                    raw_document_id, kind, target_source_id, target_lang, target_external_ref, label
                  )
                  values
                  """);

        var parameters = new DynamicParameters();
        parameters.Add("rawDocumentId", rawDocumentId);
        parameters.Add("targetSourceId", targetSourceId);

        for (var i = 0; i < links.Count; i++)
        {
            if (i > 0)
                sb.Append(',');

            sb.Append(CultureInfo.InvariantCulture, $" (@rawDocumentId, 'internal', @targetSourceId, @tl{i}, @ter{i}, @lb{i})");
            parameters.Add($"tl{i}", links[i].targetLang);
            parameters.Add($"ter{i}", links[i].targetExternalRef);
            parameters.Add($"lb{i}", links[i].label);
        }

        sb.Append(" on conflict (raw_document_id, kind, target_source_id, target_lang, target_external_ref) do nothing");

        using var db = dbf.Create();
        await db.ExecuteAsync(new CommandDefinition(sb.ToString(), parameters, cancellationToken: ct));
    }

    public async Task InsertExternalLinks(
        long rawDocumentId,
        IReadOnlyList<(string url, string? label)> links,
        CancellationToken ct)
    {
        if (links.Count == 0)
            return;

        var sb = new StringBuilder();
        sb.Append("""
                  insert into public.raw_document_links(
                    raw_document_id, kind, url, label
                  )
                  values
                  """);

        var parameters = new DynamicParameters();
        parameters.Add("rawDocumentId", rawDocumentId);

        for (var i = 0; i < links.Count; i++)
        {
            if (i > 0)
                sb.Append(',');

            sb.Append(CultureInfo.InvariantCulture, $" (@rawDocumentId, 'external', @url{i}, @lb{i})");
            parameters.Add($"url{i}", links[i].url);
            parameters.Add($"lb{i}", links[i].label);
        }

        sb.Append(" on conflict (raw_document_id, kind, url) do nothing");

        using var db = dbf.Create();
        await db.ExecuteAsync(new CommandDefinition(sb.ToString(), parameters, cancellationToken: ct));
    }
}
