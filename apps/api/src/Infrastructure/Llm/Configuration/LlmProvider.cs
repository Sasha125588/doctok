namespace Infrastructure.Llm.Configuration;

public enum LlmProvider
{
  OpenRouter,
  Local
}

public sealed class LlmRouteOptions
{
  public LlmProvider Provider { get; init; }
  public string Model { get; init; } = null!;
  public int TimeoutSeconds { get; init; } = 120;
  public int MaxTokens { get; init; } = 4000;
  public bool AllowFallbackToLocal { get; init; }
  public string? FallbackModel { get; init; }
}
