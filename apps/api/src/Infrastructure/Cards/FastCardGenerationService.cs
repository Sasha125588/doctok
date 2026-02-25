using Infrastructure.Persistence.Repos.Cards;
using Infrastructure.Persistence.Repos.Raw;
using Infrastructure.Persistence.Repos.Sources;

namespace Infrastructure.Cards;

public sealed class FastCardGenerationService(
    RawDocumentsRepository rawDocs,
    CardsRepository cardsRepo,
    SourcesRepository sources,
    FastCardGenerator gen)
{
    public async Task GenerateAsync(string sourceCode, string lang, string externalRef, CancellationToken ct)
    {
        var sourceId = await sources.GetSourceIdByCode(sourceCode, ct);

        var row = await rawDocs.GetForCardGeneration(sourceId, lang, externalRef, ct)
                  ?? throw new InvalidOperationException(
                      $"Raw document not found: source={sourceCode}, lang={lang}, ref={externalRef}");

        var cards = gen.Generate(row.Content)
            .Select(c => new CardInsert(c.Kind, c.Title, c.Body, c.Position));

        await cardsRepo.ReplaceForDocument(row.Id, row.Topic_Id, lang, cards, ct);
    }
}
