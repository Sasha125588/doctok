using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Llm;

public sealed class OpenRouterClient(HttpClient http)
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
            Messages: [new ChatMessage(Role: "user", Content: userMessage)]);

        using var response = await http.PostAsJsonAsync("chat/completions", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: ct);
        return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
    }

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages
    );

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content
    );

    private sealed record ChatResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<ChatChoice>? Choices
    );

    private sealed record ChatChoice(
        [property: JsonPropertyName("message")] ChatMessage? Message
    );
}
