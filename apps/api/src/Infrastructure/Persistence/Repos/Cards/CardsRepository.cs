using System.Globalization;
using System.Text;
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
        var cardList = cards as IReadOnlyList<CardInsert> ?? cards.ToList();

        using var db = dbf.Create();
        db.Open();
        using var tx = db.BeginTransaction();

        const string deleteSql = """
                                 delete from public.cards
                                 where raw_document_id = @rawDocumentId and lang = @lang
                                 """;

        await db.ExecuteAsync(new CommandDefinition(deleteSql, new { rawDocumentId, lang }, transaction: tx, cancellationToken: ct));

        if (cardList.Count > 0)
        {
            var sb = new StringBuilder();
            sb.Append("""
                      insert into public.cards(topic_id, raw_document_id, lang, kind, title, body, position)
                      values
                      """);

            var parameters = new DynamicParameters();
            parameters.Add("topicId", topicId);
            parameters.Add("rawDocumentId", rawDocumentId);
            parameters.Add("lang", lang);

            for (var i = 0; i < cardList.Count; i++)
            {
                if (i > 0)
                    sb.Append(',');

                sb.Append(CultureInfo.InvariantCulture, $" (@topicId, @rawDocumentId, @lang, @k{i}, @t{i}, @b{i}, @p{i})");
                parameters.Add($"k{i}", cardList[i].Kind);
                parameters.Add($"t{i}", cardList[i].Title);
                parameters.Add($"b{i}", cardList[i].Body);
                parameters.Add($"p{i}", cardList[i].Position);
            }

            await db.ExecuteAsync(new CommandDefinition(sb.ToString(), parameters, transaction: tx, cancellationToken: ct));
        }

        tx.Commit();
    }
}

public sealed record CardInsert(
    string Kind,
    string? Title,
    string Body,
    int Position);
