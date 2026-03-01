using System.ComponentModel.DataAnnotations;

namespace Infrastructure.GitHub;

public sealed class GitHubOptions
{
    [Required]
    public string Token { get; init; } = default!;

    public string ApiBaseUrl { get; init; } = "https://api.github.com";

    public string UserAgent { get; init; } = "DocTok";
}
