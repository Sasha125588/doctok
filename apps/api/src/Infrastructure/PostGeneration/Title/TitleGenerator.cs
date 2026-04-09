using System.Reflection;
using Infrastructure.Llm.OpenRouter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.PostGeneration.Title;

public sealed class TitleGenerator(

    // GeminiClient geminiClient,

    OpenRouterClient openRouter,
    IOptions<TitleGeneratorOptions> options,
    ILogger<TitleGenerator> logger) : ITitleGenerator
{
    private static readonly string[] _knownKinds = ["summary", "fact", "example"];

    private static readonly IReadOnlyDictionary<string, string> _prompts = LoadPrompts();

    public async Task<string?> GenerateTitleAsync(
        string kind,
        string topicTitle,
        string body,
        string lang,
        CancellationToken ct)
    {
        try
        {
            var prompt = BuildPrompt(kind, topicTitle, body, lang);

            var title = await openRouter.CompleteChatAsync(
                model:       options.Value.Model,
                maxTokens: options.Value.MaxTokens,
                userMessage: prompt,
                ct:          ct);

            if (string.IsNullOrWhiteSpace(title))
            {
                logger.LogWarning(
                    "OpenRouter returned empty title for kind={Kind} topic={Topic} lang={Lang}",
                    kind,
                    topicTitle,
                    lang);
                return null;
            }

            return title.Replace("\\\"", "\"").Trim('"');
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Title generation failed for kind={Kind} topic={Topic} lang={Lang}",
                kind,
                topicTitle,
                lang);
            return null;
        }
    }

    private static string BuildPrompt(string kind, string topicTitle, string body, string lang)
    {
        var normalizedKind = _knownKinds.Contains(kind, StringComparer.OrdinalIgnoreCase)
            ? kind.ToLowerInvariant()
            : "summary";

        var langName = lang.ToLowerInvariant() switch
        {
            "ru" => "Russian",
            "en" => "English",
            _ => lang
        };

        return _prompts[normalizedKind]
            .Replace("{lang}",  langName)
            .Replace("{topic}", topicTitle)
            .Replace("{body}",  TrimBody(body));
    }

    private static string TrimBody(string body, int maxLength = 600)
        => body.Length <= maxLength ? body : body[..maxLength] + "…";

    private static Dictionary<string, string> LoadPrompts()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kind in _knownKinds)
        {
            var resourceName = $"{assembly.GetName().Name}.Llm.Prompts.title_{kind}.md";
            using var stream = assembly.GetManifestResourceStream(resourceName)
                               ?? throw new InvalidOperationException(
                                   $"Embedded prompt not found: {resourceName}. " +
                                   "Make sure the file is marked as EmbeddedResource in .csproj");
            using var reader = new StreamReader(stream);
            result[kind] = reader.ReadToEnd();
        }

        return result;
    }
}
