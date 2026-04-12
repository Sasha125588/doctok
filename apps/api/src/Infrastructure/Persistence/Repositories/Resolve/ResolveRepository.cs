using Dapper;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class ResolveRepository(IDbConnectionFactory dbf)
{
    public async Task<string?> FindTopicSlugForDocument(
      long sourceId,
      string lang,
      string externalRef,
      CancellationToken ct = default)
    {
      const string query = """
                          select t.slug
                          from public.raw_documents rd
                          join public.topic_documents td on td.raw_document_id = rd.id
                          join public.topics t on t.id = td.topic_id
                          where rd.source_id = @sourceId
                            and rd.lang = @lang
                            and rd.external_ref = @externalRef
                          limit 1
                          """;

      using var db = dbf.Create();
      return await db.ExecuteScalarAsync<string>(
          new CommandDefinition(query, new { sourceId, lang, externalRef }, cancellationToken: ct));
    }
}
