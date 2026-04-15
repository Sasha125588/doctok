using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Infrastructure.Llm.Abstractions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Llm.Providers.Local;

public sealed class LocalLlmClient(HttpClient http, ILogger<LocalLlmClient> logger) : ILlmTransportClient
{
    public async Task<string?> CompleteChatAsync(string model, string userMessage, int maxTokens, CancellationToken ct)
    {
        var request = new ChatRequest(
            Model:     model,
            MaxTokens: maxTokens,
            Messages:  [new ChatMessage(Role: "user", Content: userMessage)]);

        using var response = await http.PostAsJsonAsync("chat/completions", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: ct);
        var content = result?.Choices?.FirstOrDefault()?.Message?.Content.Trim();

        logger.LogDebug(
          "Local LLM response received for model={Model}, hasContent={HasContent}",
          model,
          !string.IsNullOrWhiteSpace(content));

        return content;
    }

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")]      string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("messages")]   IReadOnlyList<ChatMessage> Messages);

    private sealed record ChatResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<ChatChoice>? Choices);

    private sealed record ChatChoice(
        [property: JsonPropertyName("message")] ChatMessage? Message);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")]    string Role,
        [property: JsonPropertyName("content")] string Content);
}
