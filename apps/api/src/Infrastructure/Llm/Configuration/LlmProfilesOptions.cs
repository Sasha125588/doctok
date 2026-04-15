using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.Configuration;

public sealed class LlmProfilesOptions
{
  [Required]
  public LlmProfileOptions Default { get; init; } = default!;

  [Required]
  public LlmPostGenerationProfileOptions PostGeneration { get; init; } = default!;

  [Required]
  public LlmProfileOptions TitleGeneration { get; init; } = default!;
}
