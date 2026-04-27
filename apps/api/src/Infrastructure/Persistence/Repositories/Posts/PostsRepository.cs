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

    public async Task UpsertOriginalVariantsForDocument(
        long rawDocumentId,
        long topicId,
        string lang,
        IReadOnlyList<OriginalPostInsert> posts,
        CancellationToken ct)
    {
        await using var conn = (DbConnection)dbf.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        const string deactivateSql = """
                                     update public.posts
                                     set is_active = false
                                     where raw_document_id = @rawDocumentId and lang = @lang
                                     """;
        await conn.ExecuteAsync(new CommandDefinition(deactivateSql, new { rawDocumentId, lang }, transaction: tx, cancellationToken: ct));

        const string upsertPostSql = """
                                     insert into public.posts(
                                       topic_id, raw_document_id, lang, source_section_key, kind,
                                       title, body, body_html, position, generation_level, is_active
                                     )
                                     values (
                                       @topicId, @rawDocumentId, @lang, @sourceSectionKey, @kind,
                                       @title, @body, @bodyHtml, @position, 0, true
                                     )
                                     on conflict (raw_document_id, lang, source_section_key)
                                       where source_section_key <> ''
                                     do update set
                                       kind = excluded.kind,
                                       title = excluded.title,
                                       body = excluded.body,
                                       body_html = excluded.body_html,
                                       position = excluded.position,
                                       generation_level = 0,
                                       is_active = true
                                     returning id
                                     """;

        const string upsertVariantSql = """
                                        insert into public.post_content_variants(
                                          post_id, variant_code, title, body, body_html, provider, model, prompt_version
                                        )
                                        values (@postId, 'original', @title, @body, @bodyHtml, null, null, 'mdn-original-v1')
                                        on conflict (post_id, variant_code) do update set
                                          title = excluded.title,
                                          body = excluded.body,
                                          body_html = excluded.body_html,
                                          updated_at = now()
                                        """;

        foreach (var p in posts)
        {
            var postId = await conn.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    upsertPostSql,
                    new
                    {
                        topicId,
                        rawDocumentId,
                        lang,
                        sourceSectionKey = p.SourceSectionKey,
                        kind = p.Kind.ToString().ToLowerInvariant(),
                        title = p.Title,
                        body = p.Body,
                        bodyHtml = p.BodyHtml,
                        position = p.Position,
                    },
                    transaction: tx,
                    cancellationToken: ct));

            await conn.ExecuteAsync(
                new CommandDefinition(
                    upsertVariantSql,
                    new
                    {
                        postId,
                        title = p.Title,
                        body = p.Body,
                        bodyHtml = p.BodyHtml,
                    },
                    transaction: tx,
                    cancellationToken: ct));
        }

        const string updateTopicCountSql = """
                                           update public.topics
                                           set post_count = (
                                             select count(*) from public.posts
                                             where topic_id = @topicId and is_active = true
                                           )
                                           where id = @topicId
                                           """;
        await conn.ExecuteAsync(new CommandDefinition(updateTopicCountSql, new { topicId }, transaction: tx, cancellationToken: ct));

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

public sealed record OriginalPostInsert(
    string SourceSectionKey,
    PostKind Kind,
    string? Title,
    string Body,
    string BodyHtml,
    int Position);
