namespace Infrastructure.Sources.Common;

public interface ISourceJobHandler
{
  string SourceCode { get; }

  Task FetchRawAsync(string lang, string externalRef, CancellationToken ct);

  Task GenerateFastCardsAsync(string lang, string externalRef, CancellationToken ct);
}
