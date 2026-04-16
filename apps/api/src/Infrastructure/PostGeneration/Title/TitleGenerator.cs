using System.Reflection;
using Domain.Common;
using Infrastructure.Llm.Abstractions;
using Infrastructure.Llm.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.PostGeneration.Title;

public sealed class TitleGenerator(
    ILlmRouter llmRouter,
    IOptions<LlmProfilesOptions> opts,
    ILogger<TitleGenerator> logger)
{
    private static readonly HashSet<string> _validKinds = new(StringComparer.OrdinalIgnoreCase)
    {
      PostKinds.Summary,
      PostKinds.Concept,
      PostKinds.Example,
      PostKinds.Tip,
    };

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
            var profile = opts.Value.TitleGeneration;
            var prompt = BuildPrompt(kind, topicTitle, body, lang);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(profile.TimeoutSeconds));

            var title = await llmRouter.CompleteChatAsync(
                profile,
                userMessage: prompt,
                ct:          cts.Token);

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
        var normalizedKind = _validKinds.Contains(kind)
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

        foreach (var kind in _validKinds)
        {
            var resourceName = $"{assembly.GetName().Name}.PostGeneration.Title.Prompts.title_{kind}.md";
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
