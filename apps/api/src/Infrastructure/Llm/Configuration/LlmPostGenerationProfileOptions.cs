using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Llm.Configuration;

public sealed class LlmPostGenerationProfileOptions: LlmProfileOptions
{
  [Range(1, int.MaxValue)]
  public int MaxContentLength { get; init; } = default!;
}
