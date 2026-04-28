using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Shared;
using Infrastructure.Llm.Abstractions;
using Infrastructure.Llm.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.PostGeneration.Llm;

/// <summary>
/// Rewrites an existing original post into a styled variant via LLM.
/// Loads prompts from embedded .md resources: a shared base template plus
/// a per-variant style file.
/// </summary>
public sealed class LlmPostGenerator(
    ILlmRouter llmRouter,
    IOptions<LlmProfilesOptions> opts,
    ILogger<LlmPostGenerator> logger)
{
    private static readonly string _baseTemplate = LoadPrompt("rewrite_base.md");

    private static readonly Dictionary<string, string> _styleByVariant = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ai_simple"] = LoadPrompt("style_ai_simple.md"),
        ["ai_senior"] = LoadPrompt("style_ai_senior.md"),
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<RewrittenPost?> RewriteVariantAsync(
        string? originalTitle,
        string originalBody,
        string kind,
        string variantCode,
        string lang,
        CancellationToken ct)
    {
        if (!_styleByVariant.TryGetValue(variantCode, out var style))
        {
            logger.LogError("No style prompt registered for variant={Variant}", variantCode);
            return null;
        }

        var profile = opts.Value.PostGeneration;
        var langName = LanguageHelpers.ToLangName(lang);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(profile.TimeoutSeconds));

        var truncated = originalBody.Length > profile.MaxContentLength
            ? originalBody[..profile.MaxContentLength] + "\n\n[content truncated]"
            : originalBody;

        var prompt = _baseTemplate
            .Replace("{lang}",  langName,                       StringComparison.Ordinal)
            .Replace("{style}", style,                          StringComparison.Ordinal)
            .Replace("{title}", originalTitle ?? string.Empty,  StringComparison.Ordinal)
            .Replace("{kind}",  kind,                           StringComparison.Ordinal)
            .Replace("{body}",  truncated,                      StringComparison.Ordinal);

        var response = await llmRouter.CompleteChatAsync(
            profile,
            userMessage: prompt,
            ct:          cts.Token);

        if (string.IsNullOrWhiteSpace(response))
        {
            logger.LogWarning(
                "LLM returned empty response for variant={Variant} title={Title}",
                variantCode,
                originalTitle);
            return null;
        }

        try
        {
            var json = ExtractJsonObject(response.Trim());
            var item = JsonSerializer.Deserialize<LlmPostItem>(json, _jsonOptions);

            if (item is null || string.IsNullOrWhiteSpace(item.Body))
                return null;

            return new RewrittenPost(item.Title?.Trim('"').Trim(), item.Body.Trim());
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Failed to parse LLM JSON for variant={Variant} title={Title}. Length={Length}",
                variantCode,
                originalTitle,
                response.Length);
            return null;
        }
    }

    private static string ExtractJsonObject(string s)
    {
        if (s.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            var end = s.LastIndexOf("```", StringComparison.Ordinal);
            s = end > 7 ? s[7..end].Trim() : s[7..].Trim();
        }
        else if (s.StartsWith("```", StringComparison.Ordinal))
        {
            var end = s.LastIndexOf("```", StringComparison.Ordinal);
            s = end > 3 ? s[3..end].Trim() : s[3..].Trim();
        }

        var objectStart = s.IndexOf('{');
        if (objectStart > 0)
            s = s[objectStart..];

        return s;
    }

    private static string LoadPrompt(string fileName)
    {
        var assembly     = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.PostGeneration.Llm.Prompts.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded prompt not found: {resourceName}. " +
                               "Make sure the file is marked as EmbeddedResource in .csproj.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private sealed record LlmPostItem(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("body")]  string? Body);
}

public sealed record RewrittenPost(string? Title, string Body);
