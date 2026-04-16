namespace Infrastructure.Llm.Abstractions;

public interface ILlmTransportClient
{
  Task<string?> CompleteChatAsync(string model, string userMessage, int maxTokens, CancellationToken ct);
}
