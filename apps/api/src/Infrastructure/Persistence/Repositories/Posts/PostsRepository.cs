using System.Data.Common;
using System.Globalization;
using System.Text;
using Dapper;
using Domain.Posts;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class PostsRepository(IDbConnectionFactory dbf)
{
    public async Task ReplaceForDocument(
        long rawDocumentId,
        long topicId,
        string lang,
        IReadOnlyList<PostInsert> posts,
        CancellationToken ct)
    {
        await using var conn = (DbConnection)dbf.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        const string deleteSql = """
                                 delete from public.posts
                                 where raw_document_id = @rawDocumentId and lang = @lang
                                 """;

        await conn.ExecuteAsync(new CommandDefinition(deleteSql, new { rawDocumentId, lang }, transaction: tx, cancellationToken: ct));

        if (posts.Count > 0)
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

            for (var i = 0; i < posts.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');

                sb.Append(CultureInfo.InvariantCulture,
                    $" (@topicId, @rawDocumentId, @lang, @k{i}, @t{i}, @b{i}, @bh{i}, @p{i}, @gl{i})");

                parameters.Add($"k{i}",  posts[i].Kind.ToString().ToLowerInvariant());
                parameters.Add($"t{i}",  posts[i].Title);
                parameters.Add($"b{i}",  posts[i].Body);
                parameters.Add($"bh{i}", posts[i].BodyHtml);
                parameters.Add($"p{i}",  posts[i].Position);
                parameters.Add($"gl{i}", posts[i].GenerationLevel);
            }

            await conn.ExecuteAsync(new CommandDefinition(sb.ToString(), parameters, transaction: tx, cancellationToken: ct));

            const string updateTopicsCountSql = """
                                                update public.topics
                                                set post_count = @post_count
                                                where id = @topicId
                                                """;

            await conn.ExecuteAsync(new CommandDefinition(updateTopicsCountSql, new { post_count = posts.Count, topicId }, transaction: tx, cancellationToken: ct));
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
    PostKind Kind,
    string? Title,
    string Body,
    string BodyHtml,
    int Position,
    int GenerationLevel = 0);
