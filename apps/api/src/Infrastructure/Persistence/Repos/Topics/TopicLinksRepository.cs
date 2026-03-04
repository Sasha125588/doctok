using Dapper;
using Domain.Models;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Topics;

public sealed class TopicLinksRepository(IDbConnectionFactory dbf)
{
    public async Task<IReadOnlyList<TopicLink>> GetLinkedTopics(
        string slug,
        string lang,
        CancellationToken ct)
    {
        const string sql = """
                           select distinct t.slug, t.title
                           from topics t
                           join topic_documents td on td.topic_id = t.id
                           join raw_documents rd on rd.id = td.raw_document_id
                           where rd.external_ref in (
                               select l.target_external_ref
                               from raw_document_links l
                               join raw_documents src on src.id = l.raw_document_id
                               join topic_documents td2 on td2.raw_document_id = src.id
                               join topics t2 on t2.id = td2.topic_id
                               where t2.slug = @slug and src.lang = @lang
                           )
                           """;

        using var db = dbf.Create();

        return (await db.QueryAsync<TopicLink>(new CommandDefinition(sql, new { slug, lang }, cancellationToken: ct))).ToList();
    }
}
