using System.Text;
using System.Text.RegularExpressions;
using Domain.Common;

namespace Infrastructure.PostGeneration;

public sealed class FastPostGenerator
{
    private const int MinTextLength = 30;
    private const int MinCodeLength = 20;

    private static readonly Regex MdnMacro =
        new(@"\{\{[^}]*\}\}", RegexOptions.Compiled);

    private static readonly Regex SectionH2 =
        new(@"^## .+$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex SectionH3 =
        new(@"^### .+$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex CodeFenceBlock =
        new(@"```(?<lang>[a-z]*)\n(?<code>[\s\S]*?)```", RegexOptions.Compiled);

    private static readonly Regex NumberedListItem =
        new(@"^\d+\.\s", RegexOptions.Compiled);

    private static readonly Regex MarkdownFormatting =
        new(@"\[([^\]]+)\]\([^)]+\)|[*`_\[\]()]", RegexOptions.Compiled);

    private static readonly HashSet<string> SkipExact = new(StringComparer.OrdinalIgnoreCase)
    {
        "see also",
        "browser compatibility",
        "specifications",
        "смотрите также",
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
    {
        "browser compat",
        "совместимость",
        "ブラウザー互換",
        "브라우저 호환",
        "浏览器兼容",
        "瀏覽器相容",
    };

    private static readonly string[] ExampleKeywords =
    {
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
    };

    public IReadOnlyList<FastPost> Generate(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        markdown = MdnMacro.Replace(markdown, string.Empty);

        var posts = new List<FastPost>();
        var pos = 0;

        var sections = SplitBySections(markdown, SectionH2, 3);

        if (sections.Count > 0 && sections[0].Title is null)
        {
            var first = ExtractFirstParagraph(sections[0].Body);
            if (first is not null && first.Length >= MinTextLength)
                posts.Add(new FastPost(PostKinds.Summary, null, first, pos++));
        }

        foreach (var section in sections)
        {
            if (section.Title is null) continue;

            switch (ClassifySection(section.Title))
            {
                case SectionType.Skip:
                    break;

                case SectionType.Examples:
                    pos = AddExamplePosts(posts, section, pos);
                    break;

                default:
                    pos = AddContentPosts(posts, section, pos);
                    break;
            }
        }

        return posts;
    }

    private static IReadOnlyList<Section> SplitBySections(
        string text, Regex splitter, int prefixLen)
    {
        var result = new List<Section>();
        var matches = splitter.Matches(text);

        var preamble = matches.Count > 0 ? text[..matches[0].Index] : text;
        if (!string.IsNullOrWhiteSpace(preamble))
            result.Add(new Section(null, preamble.Trim()));

        for (var i = 0; i < matches.Count; i++)
        {
            var title = matches[i].Value[prefixLen..].Trim();
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
            var body = text[start..end].Trim();
            result.Add(new Section(title, body));
        }

        return result;
    }

    private static SectionType ClassifySection(string title)
    {
        if (SkipExact.Contains(title))
            return SectionType.Skip;

        var lower = title.ToLowerInvariant();

        if (SkipContains.Any(k => lower.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return SectionType.Skip;

        if (ExampleKeywords.Any(k => lower.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return SectionType.Examples;

        return SectionType.Content;
    }

    private static int AddExamplePosts(List<FastPost> posts, Section section, int pos)
    {
        var subs = SplitBySections(section.Body, SectionH3, 4);

        if (subs.Count == 0 || (subs.Count == 1 && subs[0].Title is null))
            return ExtractBlocks(posts, section.Title, section.Body, pos, codeOnly: true);

        foreach (var sub in subs)
        {
            var heading = sub.Title ?? section.Title;
            pos = ExtractBlocks(posts, heading, sub.Body, pos, codeOnly: true);
        }

        return pos;
    }

    private static int AddContentPosts(List<FastPost> posts, Section section, int pos)
    {
        var subs = SplitBySections(section.Body, SectionH3, 4);

        if (subs.Count == 0 || (subs.Count == 1 && subs[0].Title is null))
            return ExtractBlocks(posts, section.Title, section.Body, pos, codeOnly: false);

        foreach (var sub in subs)
        {
            var heading = sub.Title ?? section.Title;
            pos = ExtractBlocks(posts, heading, sub.Body, pos, codeOnly: false);
        }

        return pos;
    }

    /// <summary>
    /// Splits <paramref name="body"/> into text/code segments and emits posts.
    /// When <paramref name="codeOnly"/> is true, only code-fence segments produce
    /// posts (Example); otherwise text segments also produce Facts.
    /// </summary>
    private static int ExtractBlocks(
        List<FastPost> posts,
        string? title,
        string body,
        int pos,
        bool codeOnly)
    {
        var segments = SplitPreservingCodeFences(body);
        var consumed = new HashSet<int>();

        for (var i = 0; i < segments.Count; i++)
        {
            if (!IsCodeFence(segments[i])) continue;

            var (lang, code) = ParseCodeFence(segments[i]);
            if (code.Length < MinCodeLength) continue;

            consumed.Add(i);

            string? desc = null;
            if (i > 0
                && !consumed.Contains(i - 1)
                && !IsCodeFence(segments[i - 1])
                && !IsListBlock(segments[i - 1]))
            {
                var prev = segments[i - 1].Trim();
                if (prev.Length is > 0 and < 400)
                {
                    desc = prev;
                    consumed.Add(i - 1);
                }
            }

            var formatted = desc is not null
                ? $"{desc}\n\n{FormatCodeFence(lang, code)}"
                : FormatCodeFence(lang, code);

            posts.Add(new FastPost(PostKinds.Example, title, formatted, pos++));
        }

        if (codeOnly)
            return pos;

        for (var i = 0; i < segments.Count; i++)
        {
            if (consumed.Contains(i) || IsCodeFence(segments[i]))
                continue;

            var trimmed = segments[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            if (IsListBlock(trimmed))
            {
                foreach (var item in ExtractListItems(trimmed))
                {
                    if (item.Length >= MinTextLength && !IsBareReference(item))
                        posts.Add(new FastPost(PostKinds.Fact, title, item, pos++));
                }
            }
            else if (trimmed.Length >= MinTextLength && !IsBareReference(trimmed))
            {
                posts.Add(new FastPost(PostKinds.Fact, title, trimmed, pos++));
            }
        }

        return pos;
    }

    private static string? ExtractFirstParagraph(string text)
    {
        foreach (var block in text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
        {
            var t = block.Trim();
            if (!string.IsNullOrWhiteSpace(t) && !IsCodeFence(t) && !IsListBlock(t))
                return t;
        }

        return null;
    }

    /// <summary>
    /// Splits text by blank lines but keeps code-fence blocks intact
    /// (they may contain blank lines internally).
    /// </summary>
    private static List<string> SplitPreservingCodeFences(string text)
    {
        var segments = new List<string>();
        var current = new StringBuilder();
        var inCodeFence = false;

        foreach (var line in text.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                if (!inCodeFence)
                {
                    Flush(current, segments);
                    inCodeFence = true;
                    current.AppendLine(line);
                }
                else
                {
                    current.AppendLine(line);
                    segments.Add(current.ToString().Trim());
                    current.Clear();
                    inCodeFence = false;
                }

                continue;
            }

            if (inCodeFence)
            {
                current.AppendLine(line);
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
                Flush(current, segments);
            else
                current.AppendLine(line);
        }

        Flush(current, segments);
        return segments;
    }

    private static void Flush(StringBuilder sb, List<string> target)
    {
        if (sb.Length == 0) return;
        var t = sb.ToString().Trim();
        if (t.Length > 0)
            target.Add(t);
        sb.Clear();
    }

    private static bool IsListBlock(string s)
    {
        var f = s.TrimStart();
        return f.StartsWith("- ", StringComparison.Ordinal)
               || f.StartsWith("* ", StringComparison.Ordinal)
               || NumberedListItem.IsMatch(f);
    }

    private static IEnumerable<string> ExtractListItems(string block)
    {
        var items = new List<string>();
        var current = new StringBuilder();

        foreach (var line in block.Split('\n'))
        {
            var trimmed = line.TrimStart();

            if (trimmed.StartsWith("- ", StringComparison.Ordinal)
                || trimmed.StartsWith("* ", StringComparison.Ordinal))
            {
                FlushItem(current, items);
                current.Append(trimmed[2..]);
            }
            else if (NumberedListItem.IsMatch(trimmed))
            {
                FlushItem(current, items);
                current.Append(NumberedListItem.Replace(trimmed, "", 1));
            }
            else if (current.Length > 0 && !string.IsNullOrWhiteSpace(trimmed))
            {
                current.Append(' ').Append(trimmed);
            }
        }

        FlushItem(current, items);
        return items;
    }

    private static void FlushItem(StringBuilder sb, List<string> items)
    {
        if (sb.Length == 0) return;
        items.Add(sb.ToString().Trim());
        sb.Clear();
    }

    private static bool IsCodeFence(string s)
        => s.TrimStart().StartsWith("```", StringComparison.Ordinal);

    private static (string Lang, string Code) ParseCodeFence(string segment)
    {
        var m = CodeFenceBlock.Match(segment);
        return m.Success
            ? (m.Groups["lang"].Value, m.Groups["code"].Value.Trim())
            : ("", segment);
    }

    private static string FormatCodeFence(string lang, string code)
    {
        var prefix = string.IsNullOrEmpty(lang) ? "```" : $"```{lang}";
        return $"{prefix}\n{code}\n```";
    }

    /// <summary>
    /// Returns true for content that is just a bare reference and not
    /// meaningful text (e.g. a single type name "AbortController" or
    /// a lone markdown link "[Fetch](mdn/...)").
    /// </summary>
    private static bool IsBareReference(string s)
    {
        var plain = MarkdownFormatting.Replace(s, "$1").Trim();
        return plain.Length > 0
               && !plain.Contains(' ')
               && char.IsUpper(plain[0]);
    }

    private sealed record Section(string? Title, string Body);

    private enum SectionType { Content, Examples, Skip }
}

public sealed record FastPost(string Kind, string? Title, string Body, int Position);
