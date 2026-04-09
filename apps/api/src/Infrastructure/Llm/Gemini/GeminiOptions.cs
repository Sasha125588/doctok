using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.Gemini;

public sealed class GeminiOptions
{
  [Required]
  public string ApiKey { get; init; } = null!;
}
