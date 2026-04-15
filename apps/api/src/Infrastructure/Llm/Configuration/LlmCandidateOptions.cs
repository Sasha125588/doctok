using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.Configuration;

public sealed class LlmCandidateOptions
{
  [Required]
  public LlmProvider Provider { get; init; } = default!;

  [Required]
  public string Model { get; init; } = default!;
}
