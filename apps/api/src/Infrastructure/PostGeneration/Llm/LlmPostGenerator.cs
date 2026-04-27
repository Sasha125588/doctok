using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Posts;
using Domain.Shared;
using Infrastructure.Llm.Abstractions;
using Infrastructure.Llm.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.PostGeneration.Llm;

public sealed class LlmPostGenerator(
    ILlmRouter llmRouter,
    IOptions<LlmProfilesOptions> opts,
    ILogger<LlmPostGenerator> logger)
{
    private static readonly string _prompt = LoadPrompt();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly HashSet<string> _validKinds =
      Enum.GetValues<PostKind>()
        .Select(x => x.ToString().ToLowerInvariant())
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<GeneratedPost>> GenerateAsync(
        string content,
        string title,
        string lang,
        CancellationToken ct)
    {
        var profile = opts.Value.PostGeneration;
        var langName = LanguageHelpers.ToLangName(lang);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(profile.TimeoutSeconds));

        var truncated = content.Length > profile.MaxContentLength
            ? content[..profile.MaxContentLength] + "\n\n[content truncated]"
            : content;

        var prompt = _prompt
            .Replace("{lang}",    langName,  StringComparison.Ordinal)
            .Replace("{title}",   title,     StringComparison.Ordinal)
            .Replace("{content}", truncated, StringComparison.Ordinal);

        var response = await llmRouter.CompleteChatAsync(
            profile,
            userMessage: prompt,
            ct:          cts.Token);

        if (string.IsNullOrWhiteSpace(response))
        {
            logger.LogWarning("LLM returned empty response for doc={Title}", title);
            return [];
        }

        return ParseResponse(response, title);
    }

    public async Task<RewrittenPost?> RewriteVariantAsync(
        string? originalTitle,
        string originalBody,
        string kind,
        string variantCode,
        string lang,
        CancellationToken ct)
    {
        var profile = opts.Value.PostGeneration;
        var langName = LanguageHelpers.ToLangName(lang);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(profile.TimeoutSeconds));

        var truncated = originalBody.Length > profile.MaxContentLength
            ? originalBody[..profile.MaxContentLength] + "\n\n[content truncated]"
            : originalBody;

        var prompt = string.Create(
            CultureInfo.InvariantCulture,
            $$"""
            Rewrite this existing developer learning card in {{langName}}.
            Variant: {{variantCode}}
            Keep the same meaning. Do not add unsupported facts.
            Return ONLY JSON:
            { "title": "...", "body": "..." }

            Original title: {{originalTitle ?? string.Empty}}
            Original kind: {{kind}}
            Original body:
            {{truncated}}
            """);

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

    private IReadOnlyList<GeneratedPost> ParseResponse(string raw, string docTitle)
    {
        try
        {
            var json  = ExtractJsonArray(raw.Trim());

            var items = JsonSerializer.Deserialize<List<LlmPostItem>>(json, _jsonOptions);

            if (items is null or { Count: 0 })
            {
                logger.LogWarning("LLM returned empty/null posts array for doc={Title}", docTitle);
                return [];
            }

            return items
                .Where(x => !string.IsNullOrWhiteSpace(x.Kind)
                             && !string.IsNullOrWhiteSpace(x.Title)
                             && !string.IsNullOrWhiteSpace(x.Body)
                             && _validKinds.Contains(x.Kind!))
                .Select((x, i) => new GeneratedPost(
                    Enum.Parse<PostKind>(x.Kind!, ignoreCase: true),
                    x.Title!.Trim('"').Trim(),
                    x.Body!.Trim(),
                    i))
                .ToList();
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Failed to parse LLM JSON for doc={Title}. ResponseLength={ResponseLength}",
                docTitle,
                raw.Length);
            return [];
        }
    }

    private static string ExtractJsonArray(string s)
    {
        if (s.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            var end = s.LastIndexOf("```", StringComparison.Ordinal);
            s = end > 7 ? s[7..end].Trim() : s[7..].Trim();
        }
        else if (s.StartsWith("```markdown", StringComparison.Ordinal))
        {
            var end = s.LastIndexOf("```", StringComparison.Ordinal);
            s = end > 11 ? s[11..end].Trim() : s[11..].Trim();
        }
        else if (s.StartsWith("```", StringComparison.Ordinal))
        {
            var end = s.LastIndexOf("```", StringComparison.Ordinal);
            s = end > 3 ? s[3..end].Trim() : s[3..].Trim();
        }

        var arrayStart = s.IndexOf('[');
        if (arrayStart > 0)
            s = s[arrayStart..];

        return s;
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

    private static string LoadPrompt()
    {
        var assembly     = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.PostGeneration.Llm.Prompts.generate_posts.md";

        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded prompt not found: {resourceName}. " +
                               "Make sure the file is marked as EmbeddedResource in .csproj.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private sealed record LlmPostItem(
        [property: JsonPropertyName("kind")]  string? Kind,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("body")]  string? Body);
}

public sealed record RewrittenPost(string? Title, string Body);
