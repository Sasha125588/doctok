using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.Providers.Local;

public sealed class LocalLlmOptions
{
  [Required]
  public Uri BaseUrl { get; init; } = default!;

  public string ApiKey { get; init; } = default!;

  [Range(1, 600)]
  public int TimeoutSeconds { get; init; } = default!;
}
