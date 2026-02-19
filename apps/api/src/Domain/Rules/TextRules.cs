using System.Text.RegularExpressions;

namespace Domain.Rules;

public static class TextRules
{
    private static readonly Regex NonSlug = new(@"[^a-z0-9\-]+", RegexOptions.Compiled);

    public static string TopicSlugFromExternalRef(string sourceCode, string externalRef)
    {
        var s = $"{sourceCode}-{externalRef}".ToLowerInvariant();
        s = s.Replace('/', '-').Replace('.', '-');
        s = s.Replace("(", "").Replace(")", "");
        s = NonSlug.Replace(s, "-");
        s = s.Trim('-');
        while (s.Contains("--")) s = s.Replace("--", "-");
        return s;
    }
}