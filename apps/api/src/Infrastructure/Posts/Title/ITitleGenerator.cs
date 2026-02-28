namespace Infrastructure.Posts.Title;

public interface ITitleGenerator
{
  Task<string?> GenerateTitleAsync(string kind, string topicTitle, string body, string lang, CancellationToken ct);
}
