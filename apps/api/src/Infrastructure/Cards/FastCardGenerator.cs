using System.Text.RegularExpressions;

namespace Infrastructure.Cards;

public sealed class FastCardGenerator
{
    private static readonly Regex CodeBlock =
        new (@"```(?<lang>\w+)?\n(?<code>.*?)```", RegexOptions.Singleline);

    public IReadOnlyList<FastCard> Generate(string markdown)
    {
    ArgumentNullException.ThrowIfNull(markdown);

    var cards = new List<FastCard>();
    var pos = 0;

    // summary
    var paragraphs = markdown.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
    if (paragraphs.Length > 0)
    {
        cards.Add(new FastCard("summary", null, Clean(paragraphs[0]), pos++));
    }

    // examples (code blocks)
    foreach (Match m in CodeBlock.Matches(markdown))
    {
        var code = m.Groups["code"].Value.Trim();
        if (code.Length > 20)
        {
            cards.Add(new FastCard("example", "Example", code, pos++));
        }
    }

    // facts
    foreach (var line in markdown.Split('\n'))
    {
        var t = line.Trim();
        if (t.StartsWith("- ") || t.StartsWith("* "))
        {
            cards.Add(new FastCard("fact", null, Clean(t[2..]), pos++));
        }
    }

    return cards;
    }

    private static string Clean(string s)
    {
        return s.Replace("`", "").Trim();
    }
}

public sealed record FastCard(string Kind, string? Title, string Body, int Position);
