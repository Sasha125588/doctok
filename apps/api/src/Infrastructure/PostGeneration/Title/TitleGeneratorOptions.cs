using System.ComponentModel.DataAnnotations;

namespace Infrastructure.PostGeneration.Title;

public sealed class TitleGeneratorOptions
{
  [Required]
  public string Model { get; init; } = default!;

  public int MaxTokens { get; init; } = 25;
}
