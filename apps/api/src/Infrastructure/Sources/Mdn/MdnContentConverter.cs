using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Infrastructure.Sources.Mdn;

public sealed record ExtractedLink(
    string Kind,              // "internal"|"external"
    string? TargetLang,
    string? TargetExternalRef,
    string? Url,
    string? Label);

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
        if (string.IsNullOrWhiteSpace(html))
            return "";

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var sb = new StringBuilder();
        ProcessNode(doc.DocumentNode, sb, links);

        return MultipleNewlines.Replace(sb.ToString(), "\n\n").Trim();
    }

    private static void ProcessNode(HtmlNode node, StringBuilder sb, List<ExtractedLink> links)
    {
        foreach (var child in node.ChildNodes)
        {
            switch (child.NodeType)
            {
                case HtmlNodeType.Text:
                    var text = WebUtility.HtmlDecode(child.InnerText);
                    sb.Append(text);
                    break;

                case HtmlNodeType.Element:
                    ProcessElement(child, sb, links);
                    break;
            }
        }
    }

    private static void ProcessElement(HtmlNode el, StringBuilder sb, List<ExtractedLink> links)
    {
        var tag = el.Name.ToLowerInvariant();

        switch (tag)
        {
            case "div":
            {
                var cls = el.GetAttributeValue("class", "");
                if (cls.Contains("notecard", StringComparison.OrdinalIgnoreCase))
                    return; // skip notecards entirely

                ProcessNode(el, sb, links);
                break;
            }

            case "p":
                ProcessNode(el, sb, links);
                sb.AppendLine();
                break;

            case "h2":
                sb.Append("## ");
                ProcessNode(el, sb, links);
                sb.AppendLine();
                break;

            case "h3":
                sb.Append("### ");
                ProcessNode(el, sb, links);
                sb.AppendLine();
                break;

            case "h4":
                sb.Append("#### ");
                ProcessNode(el, sb, links);
                sb.AppendLine();
                break;

            case "li":
                sb.Append("- ");
                ProcessNode(el, sb, links);
                sb.AppendLine();
                break;

            case "ul":
            case "ol":
            case "dl":
                ProcessNode(el, sb, links);
                break;

            case "dt":
                sb.Append("**");
                ProcessNode(el, sb, links);
                sb.Append("**");
                sb.AppendLine();
                break;

            case "dd":
                sb.Append("  ");
                ProcessNode(el, sb, links);
                sb.AppendLine();
                break;

            case "pre":
            {
                var codeNode = el.SelectSingleNode(".//code");
                if (codeNode != null)
                {
                    var lang = ExtractCodeLang(codeNode);
                    sb.AppendLine(CultureInfo.InvariantCulture, $"```{lang}");
                    sb.AppendLine(WebUtility.HtmlDecode(codeNode.InnerText).Trim());
                    sb.AppendLine("```");
                }
                else
                {
                    sb.AppendLine("```");
                    sb.AppendLine(WebUtility.HtmlDecode(el.InnerText).Trim());
                    sb.AppendLine("```");
                }

                break;
            }

            case "code":
            {
                // Inline code (not inside <pre>)
                if (el.ParentNode?.Name.ToLowerInvariant() != "pre")
                {
                    sb.Append('`');
                    sb.Append(WebUtility.HtmlDecode(el.InnerText));
                    sb.Append('`');
                }

                break;
            }

            case "a":
            {
                var href = el.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href))
                {
                    var slug = ExtractInternalSlug(href);
                    if (slug != null)
                    {
                        sb.Append('[');
                        ProcessNode(el, sb, links);
                        sb.Append("](");
                        sb.Append(slug);
                        sb.Append(')');
                    }
                    else
                    {
                        ProcessNode(el, sb, links);
                    }

                    TryAddLink(href, links);
                }
                else
                {
                    ProcessNode(el, sb, links);
                }

                break;
            }

            case "strong" or "b":
                sb.Append("**");
                ProcessNode(el, sb, links);
                sb.Append("**");
                break;

            case "em" or "i":
                sb.Append('*');
                ProcessNode(el, sb, links);
                sb.Append('*');
                break;

            case "br":
                sb.AppendLine();
                break;

            case "table" or "thead" or "tbody" or "tr" or "th" or "td"
                or "section" or "article" or "span" or "abbr"
                or "blockquote" or "figure" or "figcaption":
                ProcessNode(el, sb, links);
                break;

            default:
                ProcessNode(el, sb, links);
                break;
        }
    }

    private static string ExtractCodeLang(HtmlNode codeNode)
    {
        var cls = codeNode.GetAttributeValue("class", "");
        if (string.IsNullOrEmpty(cls))
            return "";

        // class="language-js" or "brush: js" patterns
        foreach (var part in cls.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.StartsWith("language-", StringComparison.OrdinalIgnoreCase))
                return part["language-".Length..];

            if (part is "js" or "css" or "html" or "json" or "xml" or "python"
                or "typescript" or "bash" or "sh" or "wasm" or "http" or "sql")
                return part;
        }

        return "";
    }

    private static void TryAddLink(string href, List<ExtractedLink> links)
    {
        var url = href.Trim();
        var hash = url.IndexOf('#');
        if (hash >= 0)
            url = url[..hash];

        if (string.IsNullOrWhiteSpace(url))
            return;

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
                _ => langRaw.ToLowerInvariant(),
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
        if (hash >= 0)
            url = url[..hash];
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (url.StartsWith("https://developer.mozilla.org", StringComparison.OrdinalIgnoreCase))
            url = new Uri(url).AbsolutePath;

        var docsIdx = url.IndexOf("/docs/", StringComparison.OrdinalIgnoreCase);
        if (docsIdx < 0)
            return null;

        var slug = url[(docsIdx + "/docs/".Length)..].Trim('/');
        return string.IsNullOrWhiteSpace(slug) ? null : slug;
    }
}
