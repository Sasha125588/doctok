namespace Infrastructure.Jobs;

public interface ISourceJobHandler
{
  Task FetchRawAsync(string lang, string externalRef, CancellationToken ct);

  Task GenerateFastPostsAsync(string lang, string externalRef, CancellationToken ct);
}
