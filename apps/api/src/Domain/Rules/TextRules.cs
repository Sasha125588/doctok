using System.Text.RegularExpressions;

namespace Domain.Rules;

public static class TextRules
{
    private static readonly Regex NonSlug = new (@"[^a-z0-9\-]+", RegexOptions.Compiled);
    private static readonly Regex MultipleDashes = new (@"-{2,}", RegexOptions.Compiled);

    public static string TopicSlugFromExternalRef(string sourceCode, string externalRef)
    {
        var s = $"{sourceCode}-{externalRef}".ToLowerInvariant();
        s = s.Replace('/', '-').Replace('.', '-');
        s = s.Replace("(", "").Replace(")", "");
        s = NonSlug.Replace(s, "-");
        s = MultipleDashes.Replace(s, "-");
        s = s.Trim('-');

        return s;
    }
}
