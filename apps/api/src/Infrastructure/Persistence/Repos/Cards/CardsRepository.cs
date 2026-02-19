using Dapper;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Cards;

public sealed class CardsRepository(IDbConnectionFactory dbf)
{
    public async Task ReplaceForDocument(
        long rawDocumentId,
        long topicId,
        string lang,
        IEnumerable<CardInsert> cards,
        CancellationToken ct)
    {
        using var db = dbf.Create();

        await db.ExecuteAsync("""
                                  delete from public.cards
                                  where raw_document_id = @rawDocumentId and lang = @lang
                              """, new { rawDocumentId, lang });

        const string insert = """
                              insert into public.cards(topic_id, raw_document_id, lang, kind, title, body, position)
                              values (@topicId, @rawDocumentId, @lang, @kind, @title, @body, @position)
                              """;

        foreach (var c in cards)
        {
            await db.ExecuteAsync(insert, new {
                topicId,
                rawDocumentId,
                lang,
                kind = c.Kind,
                title = c.Title,
                body = c.Body,
                position = c.Position
            });
        }
    }
}

public sealed record CardInsert(
    string Kind,
    string? Title,
    string Body,
    int Position);