using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.Providers.Gemini;

public sealed class GeminiOptions
{
  [Required]
  public string ApiKey { get; init; } = null!;
}
