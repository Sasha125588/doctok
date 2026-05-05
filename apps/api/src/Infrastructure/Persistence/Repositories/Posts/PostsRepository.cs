using Dapper;
using Domain.Posts;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class PostsRepository(IDbConnectionFactory dbf)
{
    public async Task UpsertOriginalVariantsForDocument(
        long rawDocumentId,
        long topicId,
        string lang,
        IReadOnlyList<OriginalPostInsert> posts,
        CancellationToken ct)
    {
        if (posts.Count == 0)
            return;

        await using var conn = dbf.Create();
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
                                       title, body, body_html, position, is_active
                                     )
                                     values (
                                       @topicId, @rawDocumentId, @lang, @sourceSectionKey, @kind,
                                       @title, @body, @bodyHtml, @position, true
                                     )
                                     on conflict (raw_document_id, lang, source_section_key)
                                       where source_section_key <> ''
                                     do update set
                                       kind = excluded.kind,
                                       title = excluded.title,
                                       body = excluded.body,
                                       body_html = excluded.body_html,
                                       position = excluded.position,
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

    public async Task<bool> ActivePostsExistForDocument(
        long rawDocumentId,
        string lang,
        CancellationToken ct)
    {
        const string sql = """
                           select exists(
                             select 1
                             from public.posts
                             where raw_document_id = @rawDocumentId
                               and lang = @lang
                               and is_active = true
                           )
                           """;

        await using var db = dbf.Create();
        return await db.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { rawDocumentId, lang }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<PostOriginalForVariant>> GetOriginalVariantsForDocument(
        long rawDocumentId,
        string lang,
        CancellationToken ct)
    {
        const string sql = """
                           select p.id as post_id,
                                  p.kind,
                                  p.position,
                                  v.title,
                                  v.body
                           from public.posts p
                           join public.post_content_variants v
                             on v.post_id = p.id
                            and v.variant_code = 'original'
                           where p.raw_document_id = @rawDocumentId
                             and p.lang = @lang
                             and p.is_active = true
                           order by p.position, p.id
                           """;

        await using var db = dbf.Create();
        return (await db.QueryAsync<PostOriginalForVariant>(
            new CommandDefinition(sql, new { rawDocumentId, lang }, cancellationToken: ct))).ToList();
    }

    public async Task<PostContentView?> GetContent(
        long postId,
        string variantCode,
        CancellationToken ct)
    {
        const string sql = """
                           select
                             p.id as post_id,
                             coalesce(requested_variant.variant_code, original_variant.variant_code, 'original') as variant_code,
                             coalesce(requested_variant.title, original_variant.title, p.title, '') as title,
                             coalesce(requested_variant.body, original_variant.body, p.body) as body,
                             coalesce(requested_variant.body_html, original_variant.body_html, p.body_html) as body_html
                           from public.posts p
                           left join public.post_content_variants requested_variant
                             on requested_variant.post_id = p.id
                            and requested_variant.variant_code = @variantCode
                           left join public.post_content_variants original_variant
                             on original_variant.post_id = p.id
                            and original_variant.variant_code = 'original'
                           where p.id = @postId
                             and p.is_active = true
                           """;

        await using var db = dbf.Create();
        return await db.QuerySingleOrDefaultAsync<PostContentView>(
            new CommandDefinition(sql, new { postId, variantCode }, cancellationToken: ct));
    }

    public async Task UpsertContentVariant(
        long postId,
        string variantCode,
        string? title,
        string body,
        string bodyHtml,
        string? provider,
        string? model,
        string promptVersion,
        CancellationToken ct)
    {
        const string sql = """
                           insert into public.post_content_variants(
                             post_id, variant_code, title, body, body_html, provider, model, prompt_version
                           )
                           values (
                             @postId, @variantCode, @title, @body, @bodyHtml, @provider, @model, @promptVersion
                           )
                           on conflict (post_id, variant_code) do update set
                             title = excluded.title,
                             body = excluded.body,
                             body_html = excluded.body_html,
                             provider = excluded.provider,
                             model = excluded.model,
                             prompt_version = excluded.prompt_version,
                             updated_at = now()
                           """;

        await using var db = dbf.Create();
        await db.ExecuteAsync(new CommandDefinition(
            sql,
            new { postId, variantCode, title, body, bodyHtml, provider, model, promptVersion },
            cancellationToken: ct));
    }
}

public sealed record OriginalPostInsert(
    string SourceSectionKey,
    PostKind Kind,
    string? Title,
    string Body,
    string BodyHtml,
    int Position);

public sealed record PostOriginalForVariant(
    long PostId,
    string Kind,
    int Position,
    string? Title,
    string Body);
