using System.Net;
using Infrastructure.Llm.Abstractions;
using Infrastructure.Llm.Configuration;
using Infrastructure.Llm.Providers.Local;
using Infrastructure.Llm.Providers.OpenRouter;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Llm.Routing;

/// <summary>
/// Виконує запити до LLM із підтримкою fallback між кількома провайдерами та моделями.
/// Повертає першу успішну відповідь або викидає виняток, якщо всі кандидати недоступні.
/// </summary>
public sealed class LlmRouter(
    OpenRouterClient openRouterClient,
    LocalLlmClient localClient,
    ILogger<LlmRouter> logger) : ILlmRouter
{
    public async Task<string?> CompleteChatAsync(LlmProfileOptions profile, string userMessage, CancellationToken ct)
    {
        Exception? lastException = null;

        for (int i = 0; i < profile.Candidates.Count; i++)
        {
            var candidate = profile.Candidates[i];

            try
            {
              logger.LogInformation(
                "Trying LLM candidate {Index}/{Total}: provider={Provider}, model={Model}",
                i + 1,
                profile.Candidates.Count,
                candidate.Provider,
                candidate.Model);

              var client = GetClient(candidate.Provider);

              var result = await client.CompleteChatAsync(candidate.Model, userMessage, profile.MaxTokens, ct);

              if (string.IsNullOrWhiteSpace(result))
              {
                logger.LogWarning(
                  "LLM candidate returned empty response: provider={Provider}, model={Model}",
                  candidate.Provider,
                  candidate.Model);

                if (i < profile.Candidates.Count - 1)
                  continue; // якщо повернуло пусту відповідь -> try next candidate

                throw new InvalidOperationException("All LLM candidates returned empty responses.");
              }

              return result;
            }
            catch (Exception ex) when (IsRecoverable(ex, ct) && i < profile.Candidates.Count - 1)
            {
              lastException = ex;

              logger.LogWarning(
                ex,
                "LLM candidate failed, trying next one: provider={Provider}, model={Model}",
                candidate.Provider,
                candidate.Model);
            }
            catch (Exception ex)
            {
              logger.LogError(
                ex,
                "LLM candidate failed with no more fallbacks: provider={Provider}, model={Model}",
                candidate.Provider,
                candidate.Model);

              throw;
            }
        }

        throw new InvalidOperationException(
          "All LLM candidates failed.",
          lastException);
    }

    private ILlmTransportClient GetClient(LlmProvider provider) =>
      provider switch
      {
        LlmProvider.OpenRouter => openRouterClient,
        LlmProvider.Local => localClient,
        _ => throw new NotSupportedException($"Unsupported LLM provider: {provider}")
      };

    private static bool IsRecoverable(Exception ex, CancellationToken ct)
    {
        // Timeout or cancellation caused by the HTTP client itself (not by our CT).
        if (ex is TaskCanceledException or OperationCanceledException)
        {
            return !ct.IsCancellationRequested;
        }

        if (ex is HttpRequestException httpEx)
        {
            // null StatusCode  = connection-level error (refused, DNS, etc.)
            // 429              = rate-limited
            // 5xx              = server-side error
            return httpEx.StatusCode is null
                or HttpStatusCode.TooManyRequests
                or HttpStatusCode.InternalServerError
                or HttpStatusCode.BadGateway
                or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.GatewayTimeout;
        }

        return false;
    }
}
