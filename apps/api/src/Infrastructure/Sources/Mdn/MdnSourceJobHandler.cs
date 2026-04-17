using Domain.Sources;
using Infrastructure.Jobs;
using Infrastructure.PostGeneration.Fast;
using Infrastructure.PostGeneration.Llm;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnSourceJobHandler(
    MdnIngestionService ingestion,
    FastPostGenerationService fastPostGeneration,
    LlmPostGenerationService llmPostGeneration)
    : ISourceJobHandler
{
    public Task FetchRawAsync(string lang, string externalRef, CancellationToken ct)
        => ingestion.FetchRawAsync(lang, externalRef, ct);

    public Task GenerateFastPostsAsync(string lang, string externalRef, CancellationToken ct)
        => fastPostGeneration.GenerateAsync(SourceIds.Mdn, SourceCodes.Mdn, lang, externalRef, ct);

    public Task GenerateLlmPostsAsync(string lang, string externalRef, CancellationToken ct)
        => llmPostGeneration.EnhanceAsync(SourceIds.Mdn, SourceCodes.Mdn, lang, externalRef, ct);
}
