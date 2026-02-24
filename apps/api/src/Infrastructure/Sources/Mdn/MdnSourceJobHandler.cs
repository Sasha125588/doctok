using Domain.Common;
using Infrastructure.Cards;
using Infrastructure.Sources.Common;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnSourceJobHandler(
  MdnIngestionService ingestion,
  FastCardGenerationService cardGeneration)
  : ISourceJobHandler
{
  public string SourceCode => SourceCodes.Mdn;

  public Task FetchRawAsync(string lang, string externalRef, CancellationToken ct)
    => ingestion.FetchRawAsync(lang, externalRef, ct);

  public Task GenerateFastCardsAsync(string lang, string externalRef, CancellationToken ct)
    => cardGeneration.GenerateAsync(SourceCode, lang, externalRef, ct);
}
