namespace Infrastructure.Sources.GitHub;

public sealed class GitHubOptions
{
    public required string Token { get; init; }
    public string ApiBaseUrl { get; init; } = "https://api.github.com";

    public required string UserAgent { get; init; }
}