using Infrastructure.Llm.Configuration;

namespace Infrastructure.Llm.Abstractions;

public interface ILlmRouter
{
  Task<string?> CompleteChatAsync(LlmProfileOptions profile, string userMessage, CancellationToken ct);
}
