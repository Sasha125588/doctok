namespace Infrastructure.Sources.Common;

public interface ISourceJobHandler
{
  string SourceCode { get; }

  Task FetchRawAsync(string lang, string externalRef, CancellationToken ct);

  Task GenerateFastPostsAsync(string lang, string externalRef, CancellationToken ct);
}
