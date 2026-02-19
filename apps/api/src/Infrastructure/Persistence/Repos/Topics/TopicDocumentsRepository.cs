using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Topics;

public sealed class TopicDocumentsRepository(IDbConnectionFactory dbf)
{
    public async Task Link(long topicId, long rawDocumentId, CancellationToken ct)
    {
        const string sql = """
                           insert into public.topic_documents(topic_id, raw_document_id)
                           values (@topicId, @rawDocumentId)
                           on conflict do nothing
                           """;

        using var db = dbf.Create();
        await db.ExecuteAsync(new CommandDefinition(sql, new { topicId, rawDocumentId }, cancellationToken: ct));
    }
}