using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.Configuration;

public class LlmProfileOptions
{
  [Required]
  [MinLength(1)]
  public IReadOnlyList<LlmCandidateOptions> Candidates { get; init; } = [];

  [Range(1, 600)]
  public int TimeoutSeconds { get; init; } = 120;

  [Range(1, int.MaxValue)]
  public int MaxTokens { get; init; } = 4000;
}
