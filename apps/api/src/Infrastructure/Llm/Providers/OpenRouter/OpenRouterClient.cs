using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Infrastructure.Llm.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Llm.Providers.OpenRouter;

public sealed class OpenRouterClient(HttpClient http, ILogger<OpenRouterClient> logger) : ILlmTransportClient
{
    public async Task<string?> CompleteChatAsync(
        string model,
        string userMessage,
        int maxTokens,
        CancellationToken ct)
    {
      var request = new ChatRequest(
        Model: model,
        MaxTokens: maxTokens,
        Messages: [new ChatMessage(Role: "user", Content: userMessage)],
        Reasoning: new ReasoningOptions(false));

      using var response = await http.PostAsJsonAsync("chat/completions", request, ct);
      response.EnsureSuccessStatusCode();

      var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: ct);
      var content = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

      logger.LogDebug(
        "OpenRouter response received for model={Model}, hasContent={HasContent}",
        model,
        !string.IsNullOrWhiteSpace(content));

      return content;
    }

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("reasoning")] ReasoningOptions? Reasoning
    );

    private sealed record ChatResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<ChatChoice>? Choices
    );

    private sealed record ChatChoice(
        [property: JsonPropertyName("message")] ChatMessage? Message
    );

    private sealed record ChatMessage(
      [property: JsonPropertyName("role")] string Role,
      [property: JsonPropertyName("content")] string Content
    );

    private sealed record ReasoningOptions(
      [property: JsonPropertyName("enabled")] bool Enabled
    );
}
