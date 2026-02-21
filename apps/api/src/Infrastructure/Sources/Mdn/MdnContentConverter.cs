using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnContentConverter
{
    private static readonly Regex MultipleNewlines = new (@"\n{3,}", RegexOptions.Compiled);

    public (string Text, IReadOnlyList<ExtractedLink> Links) Convert(MdnApiDoc doc)
    {
        var links = new List<ExtractedLink>();
        var sb = new StringBuilder();

        foreach (var section in doc.Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.SectionTitle))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"## {section.SectionTitle}");
                sb.AppendLine();
            }

            var converted = ConvertHtml(section.Content, links);
            sb.AppendLine(converted);
            sb.AppendLine();
        }

        var text = MultipleNewlines.Replace(sb.ToString(), "\n\n").Trim();

        return (text, links.DistinctBy(l => (l.Kind, l.TargetLang, l.TargetExternalRef, l.Url)).ToList());
    }

    private static string ConvertHtml(string html, List<ExtractedLink> links)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";

        var sb = new StringBuilder();
        string? currentLinkSlug = null;
        var skipDepth = 0;
        var pos = 0;
        var len = html.Length;

        while (pos < len)
        {
            var tagStart = html.IndexOf('<', pos);
            if (tagStart < 0)
            {
                if (skipDepth == 0) sb.Append(DecodeEntities(html[pos..]));
                break;
            }

            if (tagStart > pos && skipDepth == 0)
                sb.Append(DecodeEntities(html[pos..tagStart]));

            var tagEnd = html.IndexOf('>', tagStart);
            if (tagEnd < 0)
            {
                if (skipDepth == 0) sb.Append(DecodeEntities(html[tagStart..]));
                break;
            }

            var tag = html[(tagStart + 1)..tagEnd];
            var isClose = tag.StartsWith('/');
            var tagName = isClose ? tag[1..].Split(' ')[0].ToLowerInvariant() : tag.Split(' ')[0].ToLowerInvariant();

            switch (tagName)
            {
                case "div":
                    if (!isClose)
                    {
                        var cls = ExtractAttr(tag, "class") ?? "";
                        if (cls.Contains("notecard", StringComparison.OrdinalIgnoreCase) || skipDepth > 0)
                            skipDepth++;
                    }
                    else if (skipDepth > 0)
                    {
                        skipDepth--;
                    }

                    break;
                case "p":
                    if (skipDepth == 0 && isClose) sb.AppendLine();
                    break;
                case "h2":
                    if (skipDepth == 0) { if (!isClose) sb.Append("## "); else sb.AppendLine(); }
                    break;
                case "h3":
                    if (skipDepth == 0) { if (!isClose) sb.Append("### "); else sb.AppendLine(); }
                    break;
                case "h4":
                    if (skipDepth == 0) { if (!isClose) sb.Append("#### "); else sb.AppendLine(); }
                    break;
                case "li":
                    if (skipDepth == 0) { if (!isClose) sb.Append("- "); else sb.AppendLine(); }
                    break;
                case "pre":
                    if (skipDepth == 0 && isClose) sb.AppendLine("```");
                    break;
                case "code":
                    if (skipDepth == 0 && !isClose && IsInsidePre(html, tagStart)) sb.AppendLine("```");
                    break;
                case "br":
                    if (skipDepth == 0) sb.AppendLine();
                    break;
                case "a":
                    if (skipDepth > 0) break;
                    if (!isClose)
                    {
                        var href = ExtractAttr(tag, "href");
                        if (href != null)
                        {
                            var slug = ExtractInternalSlug(href);
                            if (slug != null)
                            {
                                currentLinkSlug = slug;
                                sb.Append('[');
                            }

                            TryAddLink(href, links);
                        }
                    }
                    else if (currentLinkSlug != null)
                    {
                        sb.Append("](");
                        sb.Append(currentLinkSlug);
                        sb.Append(')');
                        currentLinkSlug = null;
                    }

                    break;
            }

            pos = tagEnd + 1;
        }

        return MultipleNewlines.Replace(sb.ToString(), "\n\n").Trim();
    }

    private static bool IsInsidePre(string html, int pos)
    {
        var before = html[..pos];
        var lastPreOpen = before.LastIndexOf("<pre", StringComparison.OrdinalIgnoreCase);
        var lastPreClose = before.LastIndexOf("</pre>", StringComparison.OrdinalIgnoreCase);
        return lastPreOpen > lastPreClose;
    }

    private static string? ExtractAttr(string tag, string attr)
    {
        var search = $"{attr}=\"";
        var idx = tag.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        var start = idx + search.Length;
        var end = tag.IndexOf('"', start);
        if (end < 0) return null;
        return tag[start..end];
    }

    private static void TryAddLink(string href, List<ExtractedLink> links)
    {
        var url = href.Trim();
        var hash = url.IndexOf('#');
        if (hash >= 0) url = url[..hash];

        if (string.IsNullOrWhiteSpace(url)) return;

        if (url.StartsWith("https://developer.mozilla.org", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(url);
            url = uri.AbsolutePath;
        }

        var docsIdx = url.IndexOf("/docs/", StringComparison.OrdinalIgnoreCase);
        if (docsIdx >= 0)
        {
            var prefix = url[..docsIdx];
            var parts = prefix.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var langRaw = parts.LastOrDefault() ?? "en-US";
            var lang = langRaw.ToLowerInvariant() switch
            {
                "en-us" or "en" => "en",
                "ru" => "ru",
                _ => langRaw.ToLowerInvariant()
            };
            var externalRef = url[(docsIdx + "/docs/".Length)..].Trim('/');
            if (!string.IsNullOrWhiteSpace(externalRef))
            {
                links.Add(new ExtractedLink("internal", lang, externalRef, null, null));
            }

            return;
        }

        if (Uri.TryCreate(href, UriKind.Absolute, out _))
        {
            links.Add(new ExtractedLink("external", null, null, href, null));
        }
    }

    private static string? ExtractInternalSlug(string href)
    {
        var url = href.Trim();
        var hash = url.IndexOf('#');
        if (hash >= 0) url = url[..hash];
        if (string.IsNullOrWhiteSpace(url)) return null;

        if (url.StartsWith("https://developer.mozilla.org", StringComparison.OrdinalIgnoreCase))
            url = new Uri(url).AbsolutePath;

        var docsIdx = url.IndexOf("/docs/", StringComparison.OrdinalIgnoreCase);
        if (docsIdx < 0) return null;

        var slug = url[(docsIdx + "/docs/".Length)..].Trim('/');
        return string.IsNullOrWhiteSpace(slug) ? null : slug;
    }

    private static string DecodeEntities(string text) =>
        text
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&#39;", "'")
            .Replace("&nbsp;", " ");
}
