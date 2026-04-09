using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.OpenRouter;

public sealed class OpenRouterOptions
{
  [Required]
  public string ApiKey { get; init; } = default!;

  public string Referer { get; init; } = default!;

  public string AppName { get; init; } = default!;
}
