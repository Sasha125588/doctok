using Dapper;
using Infrastructure.Persistence.Db;
using Infrastructure.Persistence.Repos.Cards;
using Infrastructure.Persistence.Repos.Sources;

namespace Infrastructure.Cards;

public sealed class FastCardGenerationService(
    IDbConnectionFactory dbf,
    CardsRepository cardsRepo,
    SourcesRepository sources,
    FastCardGenerator gen)
{
    public async Task GenerateAsync(string sourceCode, string lang, string externalRef, CancellationToken ct)
    {
        var sourceId = await sources.GetSourceIdByCode(sourceCode, ct);

        const string query = """
                             select rd.id, rd.content, td.topic_id
                             from raw_documents rd
                             join topic_documents td on td.raw_document_id = rd.id
                             where rd.source_id = @sourceId
                               and rd.lang = @lang
                               and rd.external_ref = @externalRef
                             """;

        using var db = dbf.Create();
        var row = await db.QuerySingleAsync<DocRow>(query, new { sourceId, lang, externalRef });

        var cards = gen.Generate(row.Content)
            .Select(c => new CardInsert(c.Kind, c.Title, c.Body, c.Position));

        await cardsRepo.ReplaceForDocument(row.Id, row.Topic_Id, lang, cards, ct);
    }

    private sealed record DocRow(long Id, string Content, long Topic_Id);
}
