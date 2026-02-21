using System.Text.RegularExpressions;

namespace Infrastructure.Sources.Mdn;

public sealed record ExtractedLink(
    string Kind,              // "internal"|"external"
    string? TargetLang,
    string? TargetExternalRef,
    string? Url,
    string? Label);

public sealed class MdnLinkExtractor
{
    // markdown: [text](/en-US/docs/Web/API/AbortSignal#examples)
    private static readonly Regex MdLink = new (@"\[(?<label>[^\]]+)\]\((?<url>[^)]+)\)",
        RegexOptions.Compiled);

    public IReadOnlyList<ExtractedLink> Extract(string mdBody)
    {
        var list = new List<ExtractedLink>();

        foreach (Match m in MdLink.Matches(mdBody))
        {
            var label = m.Groups["label"].Value;
            var url = m.Groups["url"].Value;

            if (TryParseMdnDocsUrl(url, out var lang, out var externalRef))
            {
                list.Add(new ExtractedLink(
                    Kind: "internal",
                    TargetLang: lang,
                    TargetExternalRef: externalRef,
                    Url: null,
                    Label: label));
            }
            else if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                list.Add(new ExtractedLink(
                    Kind: "external",
                    TargetLang: null,
                    TargetExternalRef: null,
                    Url: url,
                    Label: label));
            }
        }

        return list
            .DistinctBy(l => (l.Kind, l.TargetLang, l.TargetExternalRef, l.Url))
            .ToList();
    }

    private static bool TryParseMdnDocsUrl(string url, out string lang, out string externalRef)
    {
        lang = "en";
        externalRef = "";

        var u = url.Trim();

        var hash = u.IndexOf('#');
        if (hash >= 0) u = u[..hash];

        // examples: /en-US/docs/Web/API/AbortSignal
        //           /ru/docs/Web/API/AbortSignal
        //           https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal
        if (u.StartsWith("https://developer.mozilla.org", StringComparison.OrdinalIgnoreCase))
        {
            u = new Uri(u).AbsolutePath;
        }

        u = u.Replace('\\', '/');

        var idx = u.IndexOf("/docs/", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return false;

        var prefix = u[..idx]; // e.g. /en-US or /ru
        var parts = prefix.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var langRaw = parts.LastOrDefault() ?? "en-US";

        lang = langRaw.ToLowerInvariant() switch
        {
            "en-us" or "en" => "en",
            "ru" => "ru",
            _ => langRaw.ToLowerInvariant()
        };

        externalRef = u[(idx + "/docs/".Length)..].Trim('/'); // Web/API/AbortSignal
        return !string.IsNullOrWhiteSpace(externalRef);
    }
}
