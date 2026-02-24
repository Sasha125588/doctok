using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Cards;

public sealed class FastCardGenerator
{
    private static readonly Regex MdnMacro =
        new (@"\{\{[^}]*\}\}", RegexOptions.Compiled);

    private static readonly Regex SectionSplit =
        new (@"^## .+$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex SubSectionSplit =
        new (@"^### .+$", RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex CodeFence =
        new (@"```[a-z]*\n(?<code>[\s\S]*?)```", RegexOptions.Compiled);

    public IReadOnlyList<FastCard> Generate(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        markdown = MdnMacro.Replace(markdown, string.Empty);

        var cards = new List<FastCard>();
        var pos = 0;

        var sections = SplitIntoSections(markdown);

        // Preamble (before first ## heading) → summary card
        if (sections.Count > 0 && sections[0].Title is null)
        {
            var first = sections[0].Body
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(first))
            {
                cards.Add(new FastCard("summary", null, Clean(first), pos++));
            }
        }

        foreach (var section in sections)
        {
            if (section.Title is null)
            {
                continue;
            }

            switch (ClassifySection(section.Title))
            {
                case SectionType.Examples:
                    pos = AddExampleCards(cards, section.Body, pos);
                    break;

                case SectionType.Description:
                    pos = AddDescriptionFacts(cards, section.Body, pos);
                    break;

                // SkipList and Other: ignore
            }
        }

        return cards;
    }

    private static IReadOnlyList<Section> SplitIntoSections(string markdown)
    {
        var result = new List<Section>();
        var matches = SectionSplit.Matches(markdown);

        // Text before first heading
        var preamble = matches.Count > 0
            ? markdown[..matches[0].Index]
            : markdown;

        if (!string.IsNullOrWhiteSpace(preamble))
        {
            result.Add(new Section(null, preamble.Trim()));
        }

        for (var i = 0; i < matches.Count; i++)
        {
            var title = matches[i].Value[3..].Trim(); // strip "## "
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : markdown.Length;
            result.Add(new Section(title, markdown[start..end].Trim()));
        }

        return result;
    }

    private static SectionType ClassifySection(string title)
    {
        var t = title.ToLowerInvariant();

        if (t.Contains("example") || t.Contains("пример"))
        {
            return SectionType.Examples;
        }

        if (t.Contains("description") || t.Contains("описание"))
        {
            return SectionType.Description;
        }

        // Reference lists: "Interfaces based on X", "Events", "See also", etc.
        if (t.Contains("interface") || t.Contains("интерфейс") ||
            t is "see also" or "смотрите также" or "browser compatibility" ||
            (t.Contains("event") && t.Length < 25) ||
            (t.Contains("событ") && t.Length < 25))
        {
            return SectionType.SkipList;
        }

        return SectionType.Other;
    }

    private static int AddExampleCards(List<FastCard> cards, string body, int pos)
    {
        var subMatches = SubSectionSplit.Matches(body);

        if (subMatches.Count == 0)
        {
            // No sub-headings — emit bare code blocks
            foreach (Match m in CodeFence.Matches(body))
            {
                var code = m.Groups["code"].Value.Trim();
                if (code.Length > 20)
                {
                    cards.Add(new FastCard("example", null, code, pos++));
                }
            }

            return pos;
        }

        for (var i = 0; i < subMatches.Count; i++)
        {
            var heading = subMatches[i].Value[4..].Trim(); // strip "### "
            var start = subMatches[i].Index + subMatches[i].Length;
            var end = i + 1 < subMatches.Count ? subMatches[i + 1].Index : body.Length;
            var chunk = body[start..end].Trim();

            // Extract code block from chunk
            var codeMatch = CodeFence.Match(chunk);
            if (!codeMatch.Success)
            {
                continue;
            }

            var code = codeMatch.Groups["code"].Value.Trim();
            if (code.Length <= 20)
            {
                continue;
            }

            // Description = text before the code fence
            var desc = chunk[..codeMatch.Index].Trim();

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(desc))
            {
                sb.AppendLine(desc);
                sb.AppendLine();
            }

            sb.AppendLine("```");
            sb.Append(code);
            sb.AppendLine();
            sb.Append("```");

            cards.Add(new FastCard("example", heading, sb.ToString().Trim(), pos++));
        }

        return pos;
    }

    private static int AddDescriptionFacts(List<FastCard> cards, string body, int pos)
    {
        foreach (var line in body.Split('\n'))
        {
            var t = line.Trim();
            if (!t.StartsWith("- ") && !t.StartsWith("* "))
            {
                continue;
            }

            var text = Clean(t[2..]);

            if (text.Length < 15)
            {
                continue;
            }

            if (IsBareTypeName(text))
            {
                continue;
            }

            cards.Add(new FastCard("fact", null, text, pos++));
        }

        return pos;
    }

    // Returns true for tokens like "WheelEvent", "AbortController", "DOMException"
    private static bool IsBareTypeName(string s) =>
        !s.Contains(' ') && s.Length > 0 && char.IsUpper(s[0]);

    private static string Clean(string s) =>
        s.Replace("`", string.Empty, StringComparison.Ordinal).Trim();

    private sealed record Section(string? Title, string Body);

    private enum SectionType
    {
        Examples,
        Description,
        SkipList,
        Other,
    }
}

public sealed record FastCard(string Kind, string? Title, string Body, int Position);
