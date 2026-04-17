using System.Text;
using Domain.Posts;

namespace Infrastructure.PostGeneration.Fast;

/// <summary>
/// Level-0 post generator: each H2 section in the markdown becomes one post.
/// No LLM is involved — this runs instantly and always succeeds.
/// Posts generated here use <c>GenerationLevel = 0</c> and are later
/// replaced by Level-2 LLM posts once the LLM job completes.
/// </summary>
public sealed class FastPostGenerator
{
    private static readonly HashSet<string> SkipSections = new(StringComparer.OrdinalIgnoreCase)
    {
        "see also",
        "browser compatibility",
        "specifications",
        "смотрите также",
        "смотри(те) также",
        "смотри также",
        "см. также",
        "совместимость с браузерами",
        "спецификации",
        "関連情報",
        "ブラウザーの互換性",
        "仕様書",
        "같이 보기",
        "브라우저 호환성",
        "명세서",
        "参见",
        "浏览器兼容性",
        "规范",
        "參見",
        "瀏覽器相容性",
        "規範",
        "voir aussi",
        "compatibilité des navigateurs",
        "spécifications",
        "siehe auch",
        "browser-kompatibilität",
        "spezifikationen",
        "véase también",
        "compatibilidad con navegadores",
        "especificaciones",
        "veja também",
        "compatibilidade com navegadores",
        "especificações",
    };

    private static readonly string[] SkipContains =
    [
        "browser compat",
        "совместимость",
        "ブラウザー互換",
        "브라우저 호환",
        "浏览器兼容",
        "瀏覽器相容",
    ];

    private static readonly string[] ExampleKeywords =
    [
        "example",
        "пример",
        "примеры",
        "例",
        "示例",
        "사용 예",
        "exemple",
        "beispiel",
        "ejemplo",
        "exemplo",
    ];

    public IReadOnlyList<GeneratedPost> Generate(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var sections = SplitH2Sections(markdown);
        var posts    = new List<GeneratedPost>();
        var pos      = 0;

        foreach (var (title, body) in sections)
        {
            var trimmedBody = body.Trim();
            if (string.IsNullOrWhiteSpace(trimmedBody)) continue;
            if (title is not null && ShouldSkip(title)) continue;

            var kind = ClassifySection(title, pos);
            posts.Add(new GeneratedPost(kind, title, trimmedBody, pos++));
        }

        return posts;
    }

    private static bool ShouldSkip(string title)
    {
        if (SkipSections.Contains(title))
            return true;

        var lower = title.ToLowerInvariant();
        return SkipContains.Any(k => lower.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static PostKind ClassifySection(string? title, int position)
    {
        if (position == 0 || title is null)
            return PostKind.Summary;

        var lower = title.ToLowerInvariant();

        if (ExampleKeywords.Any(k => lower.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return PostKind.Example;

        return PostKind.Concept;
    }

    /// <summary>
    /// Splits markdown by H2 headings (<c>## Title</c>).
    /// The preamble before the first H2 becomes a section with a null title.
    /// Each section body includes all content up to (but not including) the next H2,
    /// so H3 subsections stay together with their parent H2.
    /// </summary>
    private static List<(string? Title, string Body)> SplitH2Sections(string markdown)
    {
        var result       = new List<(string?, string)>();
        var currentTitle = (string?)null;
        var currentBody  = new StringBuilder();

        foreach (var line in markdown.Split('\n'))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                FlushSection(result, currentTitle, currentBody);
                currentTitle = line[3..].Trim();
                currentBody.Clear();
            }
            else
            {
                currentBody.AppendLine(line);
            }
        }

        FlushSection(result, currentTitle, currentBody);
        return result;
    }

    private static void FlushSection(
        List<(string?, string)> result,
        string? title,
        StringBuilder body)
    {
        var text = body.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(text))
            result.Add((title, text));
    }
}
