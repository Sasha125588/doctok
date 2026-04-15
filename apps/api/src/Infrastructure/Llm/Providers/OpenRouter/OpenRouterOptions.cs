using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.Providers.OpenRouter;

public sealed class OpenRouterOptions
{
  [Required]
  public Uri BaseUrl { get; init; } = default!;

  [Required]
  public string ApiKey { get; init; } = default!;

  public string Referer { get; init; } = default!;

  public string AppName { get; init; } = default!;

  [Range(1, 600)]
  public int TimeoutSeconds { get; init; } = default!;
}
