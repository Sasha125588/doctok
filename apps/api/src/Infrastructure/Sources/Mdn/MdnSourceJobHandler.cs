using Domain.Common;
using Infrastructure.Jobs;
using Infrastructure.PostGeneration;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnSourceJobHandler(
  MdnIngestionService ingestion,
  FastPostGenerationService postGeneration)
  : ISourceJobHandler
{
  public Task FetchRawAsync(string lang, string externalRef, CancellationToken ct)
    => ingestion.FetchRawAsync(lang, externalRef, ct);

  public Task GenerateFastPostsAsync(string lang, string externalRef, CancellationToken ct)
    => postGeneration.GenerateAsync(SourceCodes.Mdn, lang, externalRef, ct);
}
