using Domain.Common;
using Infrastructure.Jobs;
using Infrastructure.PostGeneration;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnSourceJobHandler(
  MdnIngestionService ingestion,
  FastPostGenerationService postGeneration)
  : ISourceJobHandler
{
  public string SourceCode => SourceCodes.Mdn;

  public Task FetchRawAsync(string lang, string externalRef, CancellationToken ct)
    => ingestion.FetchRawAsync(lang, externalRef, ct);

  public Task GenerateFastPostsAsync(string lang, string externalRef, CancellationToken ct)
    => postGeneration.GenerateAsync(SourceCode, lang, externalRef, ct);
}
