using System.Data.Common;
using System.Globalization;
using System.Text;
using Dapper;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class PostsRepository(IDbConnectionFactory dbf)
{
    public async Task ReplaceForDocument(
        long rawDocumentId,
        long topicId,
        string lang,
        IEnumerable<PostInsert> posts,
        CancellationToken ct)
    {
        var postList = posts as IReadOnlyList<PostInsert> ?? posts.ToList();

        using var db = dbf.Create();
        await ((DbConnection)db).OpenAsync(ct);
        await using var tx = await ((DbConnection)db).BeginTransactionAsync(ct);

        const string deleteSql = """
                                 delete from public.posts
                                 where raw_document_id = @rawDocumentId and lang = @lang
                                 """;

        await db.ExecuteAsync(new CommandDefinition(deleteSql, new { rawDocumentId, lang }, transaction: tx, cancellationToken: ct));

        if (postList.Count > 0)
        {
            var sb = new StringBuilder();
            sb.Append("""
                      insert into public.posts(topic_id, raw_document_id, lang, kind, title, body, body_html, position, generation_level)
                      values
                      """);

            var parameters = new DynamicParameters();
            parameters.Add("topicId", topicId);
            parameters.Add("rawDocumentId", rawDocumentId);
            parameters.Add("lang", lang);

            for (var i = 0; i < postList.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');

                sb.Append(CultureInfo.InvariantCulture,
                    $" (@topicId, @rawDocumentId, @lang, @k{i}, @t{i}, @b{i}, @bh{i}, @p{i}, @gl{i})");

                parameters.Add($"k{i}",  postList[i].Kind);
                parameters.Add($"t{i}",  postList[i].Title);
                parameters.Add($"b{i}",  postList[i].Body);
                parameters.Add($"bh{i}", postList[i].BodyHtml);
                parameters.Add($"p{i}",  postList[i].Position);
                parameters.Add($"gl{i}", postList[i].GenerationLevel);
            }

            await db.ExecuteAsync(new CommandDefinition(sb.ToString(), parameters, transaction: tx, cancellationToken: ct));
        }

        await tx.CommitAsync(ct);
    }

    public async Task<int> GetMinGenerationLevel(long rawDocumentId, string lang, CancellationToken ct)
    {
        const string sql = """
                           select coalesce(min(generation_level), -1)
                           from public.posts
                           where raw_document_id = @rawDocumentId
                             and lang = @lang
                           """;

        using var db = dbf.Create();
        return await db.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { rawDocumentId, lang }, cancellationToken: ct));
    }
}

public sealed record PostInsert(
    string Kind,
    string? Title,
    string Body,
    string BodyHtml,
    int Position,
    int GenerationLevel = 0);
