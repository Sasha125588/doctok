using System.Data.Common;
using System.Globalization;
using System.Text;
using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Posts;

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
                      insert into public.posts(topic_id, raw_document_id, lang, kind, title, body, position)
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

                sb.Append(CultureInfo.InvariantCulture, $" (@topicId, @rawDocumentId, @lang, @k{i}, @t{i}, @b{i}, @p{i})");
                parameters.Add($"k{i}", postList[i].Kind);
                parameters.Add($"t{i}", postList[i].Title);
                parameters.Add($"b{i}", postList[i].Body);
                parameters.Add($"p{i}", postList[i].Position);
            }

            await db.ExecuteAsync(new CommandDefinition(sb.ToString(), parameters, transaction: tx, cancellationToken: ct));
        }

        await tx.CommitAsync(ct);
    }
}

public sealed record PostInsert(
    string Kind,
    string? Title,
    string Body,
    int Position);
