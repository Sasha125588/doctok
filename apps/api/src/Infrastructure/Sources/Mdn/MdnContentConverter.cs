using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Shared;
using Domain.Sources;
using HtmlAgilityPack;

namespace Infrastructure.Sources.Mdn;

/// <summary>
/// Extracted link from an MDN article.
/// Kind: "internal" — link to another MDN doc; "external" — link to an outside URL.
/// </summary>
public sealed record ExtractedLink(
    string Kind,
    string? TargetLang,
    string? TargetExternalRef,
    string? Url,
    string? Label);

public sealed class MdnContentConverter
{
    private static readonly Regex MultipleNewlines = new(@"\n{3,}", RegexOptions.Compiled);

    // Matches "brush: js" / "brush:js" in MDN's <pre> class attribute.
    private static readonly Regex BrushLang = new(@"brush:\s*(\w+)", RegexOptions.Compiled);

    // Collapses whitespace sequences containing a newline into a single space.
    // Used to strip HTML-formatting newlines inside inline elements (e.g. <dt>).
    private static readonly Regex InlineNewlines = new(@"[ \t]*\n[ \t]*", RegexOptions.Compiled);

    private static readonly HashSet<string> KnownCodeLangs = new(StringComparer.OrdinalIgnoreCase)
    {
        "js", "css", "html", "json", "xml", "python",
        "typescript", "bash", "sh", "wasm", "http", "sql",
        "jsx", "tsx", "yaml", "toml", "c", "cpp", "csharp",
        "java", "ruby", "go", "rust", "swift", "kotlin",
    };

    public (string Text, IReadOnlyList<ExtractedLink> Links) Convert(MdnApiDoc doc)
    {
        var links = new List<ExtractedLink>();
        var sb = new StringBuilder();

        foreach (var section in doc.Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.SectionTitle))
            {
                var heading = section.IsH3 ? "###" : "##";
                sb.AppendLine(CultureInfo.InvariantCulture, $"{heading} {section.SectionTitle}");
                sb.AppendLine();
            }

            var converted = ConvertHtml(section.Content, links);
            sb.AppendLine(converted);
            sb.AppendLine();
        }

        var text = NormalizeText(sb);

        var distinct = links
            .DistinctBy(l => (l.Kind, l.TargetLang, l.TargetExternalRef, l.Url))
            .ToList();

        return (text, distinct);
    }

    private string ConvertHtml(string html, List<ExtractedLink> links)
    {
        if (string.IsNullOrWhiteSpace(html))
            return "";

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var sb = new StringBuilder();
        ProcessNode(doc.DocumentNode, sb, links);

        return NormalizeText(sb);
    }

    private void ProcessNode(HtmlNode node, StringBuilder sb, List<ExtractedLink> links)
    {
        foreach (var child in node.ChildNodes)
        {
            switch (child.NodeType)
            {
                case HtmlNodeType.Text:
                    sb.Append(WebUtility.HtmlDecode(child.InnerText));
                    break;
                case HtmlNodeType.Element:
                    ProcessElement(child, sb, links);
                    break;
            }
        }
    }

    private void ProcessElement(HtmlNode el, StringBuilder sb, List<ExtractedLink> links)
    {
        var tag = el.Name.ToLowerInvariant();

        switch (tag)
        {
            case "div":
                // Skip the MDN "example-header" div — it contains only a decorative
                // language label (<span class="language-name">js</span>) that would
                // otherwise leak as plain text before the code fence.
                // The language is extracted from <pre class="brush: js"> instead.
                if (HasClass(el, "example-header"))
                    break;
                ProcessNode(el, sb, links);
                break;

            case "p":
            {
                ProcessNode(el, sb, links);

                // Suppress trailing newline when <p> is inside <li> or <dd>
                // to avoid double blank lines (the parent element adds its own).
                var parentTag = el.ParentNode?.Name.ToLowerInvariant();
                if (parentTag is not "li" and not "dd")
                {
                    sb.AppendLine();
                }

                break;
            }

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

            case "ul":
                sb.AppendLine();
                ProcessNode(el, sb, links);
                break;

            case "dl":
                sb.AppendLine();
                RenderDefinitionList(el, sb, links);
                break;

            case "ol":
                sb.AppendLine();
                RenderOrderedList(el, sb, links);
                break;

            case "li":
                sb.Append("- ");
                ProcessNode(el, sb, links);
                sb.AppendLine();
                break;

            // <dt> and <dd> are only reached when nested outside a <dl>
            // (unusual but possible). In the normal flow they are handled
            // by RenderDefinitionList which keeps them together without blank lines.
            case "dt":
                sb.Append("**");
                ProcessNode(el, sb, links);
                sb.Append("**");
                sb.AppendLine();
                break;

            case "dd":
                sb.Append(": ");
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
                    // No <code> child — still try to get the lang from the <pre> itself.
                    var lang = ExtractLangFromPre(el);
                    sb.AppendLine(CultureInfo.InvariantCulture, $"```{lang}");
                    sb.AppendLine(WebUtility.HtmlDecode(el.InnerText).Trim());
                    sb.AppendLine("```");
                }

                break;
            }

            case "code":
                if (el.ParentNode?.Name.Equals("pre", StringComparison.OrdinalIgnoreCase) != true)
                {
                    sb.Append('`');
                    sb.Append(WebUtility.HtmlDecode(el.InnerText));
                    sb.Append('`');
                }

                break;

            case "a":
                RenderAnchor(el, sb, links);
                break;

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

            case "table":
                sb.AppendLine();
                RenderTable(el, sb, links);
                break;

            case "img":
            {
                var alt = el.GetAttributeValue("alt", "");
                if (!string.IsNullOrWhiteSpace(alt))
                    sb.Append(alt);
                break;
            }

            case "span":
                // <span class="visually-hidden"> exists only for screen readers
                // (e.g. inside MDN badge icons). Never output it as visible text.
                if (HasClass(el, "visually-hidden"))
                    break;
                ProcessNode(el, sb, links);
                break;

            case "thead" or "tbody" or "tr" or "th" or "td"
                or "section" or "article" or "abbr"
                or "blockquote" or "figure" or "figcaption"
                or "sup" or "sub" or "small" or "mark":
                ProcessNode(el, sb, links);
                break;

            default:
                ProcessNode(el, sb, links);
                break;
        }
    }

    private void RenderAnchor(HtmlNode el, StringBuilder sb, List<ExtractedLink> links)
    {
        var href = el.GetAttributeValue("href", "");
        if (string.IsNullOrEmpty(href))
            href = el.GetAttributeValue("data-href", "");

        if (string.IsNullOrEmpty(href))
        {
            ProcessNode(el, sb, links);
            return;
        }

        var label = GetPlainText(el);
        var parsed = ParseUrl(href);

        if (parsed is null)
        {
            ProcessNode(el, sb, links);
            return;
        }

        sb.Append('[');
        ProcessNode(el, sb, links);
        sb.Append("](");

        if (parsed.IsInternal)
        {
            var slug = parsed.ExternalRef!.ToLowerInvariant();
            sb.Append(SourceCodes.Mdn).Append('/').Append(slug);
            links.Add(new ExtractedLink("internal", parsed.Lang, parsed.ExternalRef, null, label));
        }
        else
        {
            sb.Append(parsed.FullUrl);
            links.Add(new ExtractedLink("external", null, null, parsed.FullUrl, label));
        }

        sb.Append(')');
    }

    /// <summary>
    /// Renders a &lt;dl&gt; by pairing each &lt;dt&gt; with its &lt;dd&gt;.
    /// Text nodes between elements (HTML-formatting newlines) are skipped so that
    /// dt and dd are never separated by a blank line — keeping them as a single
    /// block for <see cref="FastPostGenerator"/>.
    /// Output format per pair:
    /// <code>
    /// **term text**
    /// : description text
    /// </code>
    /// Multiple pairs in the same dl produce one continuous block (no blank lines).
    /// </summary>
    private void RenderDefinitionList(HtmlNode dl, StringBuilder sb, List<ExtractedLink> links)
    {
        HtmlNode? pendingDt = null;

        foreach (var child in dl.ChildNodes)
        {
            // Skip whitespace text nodes between <dt> and <dd>.
            // These are the root cause of blank-line separation in the output.
            if (child.NodeType != HtmlNodeType.Element)
                continue;

            var tag = child.Name.ToLowerInvariant();

            switch (tag)
            {
                case "dt":
                    // A new <dt> with an un-rendered previous <dt> → flush the orphan
                    if (pendingDt != null)
                    {
                        sb.Append("**");
                        sb.Append(GetInlineContent(pendingDt, links));
                        sb.AppendLine("**");
                    }

                    pendingDt = child;
                    break;

                case "dd":
                    // Render "**dt**\n: dd\n" — no blank line between them
                    sb.Append("**");
                    if (pendingDt != null)
                    {
                        sb.Append(GetInlineContent(pendingDt, links));
                        pendingDt = null;
                    }

                    sb.AppendLine("**");

                    var ddBody = new StringBuilder();
                    ProcessNode(child, ddBody, links);
                    sb.Append(": ");
                    sb.AppendLine(ddBody.ToString().Trim());
                    break;
            }
        }

        // Flush any trailing <dt> that had no <dd>
        if (pendingDt != null)
        {
            sb.Append("**");
            sb.Append(GetInlineContent(pendingDt, links));
            sb.AppendLine("**");
        }
    }

    /// <summary>
    /// Renders <paramref name="node"/> to a plain inline string:
    /// all HTML-formatting newlines are collapsed to spaces so that
    /// multi-line source like &lt;dt&gt; becomes a single clean line.
    /// </summary>
    private string GetInlineContent(HtmlNode node, List<ExtractedLink> links)
    {
        var inner = new StringBuilder();
        ProcessNode(node, inner, links);
        return InlineNewlines.Replace(inner.ToString().Trim(), " ").Trim();
    }

    private void RenderOrderedList(HtmlNode ol, StringBuilder sb, List<ExtractedLink> links)
    {
        var counter = 0;
        foreach (var child in ol.ChildNodes)
        {
            if (child.NodeType != HtmlNodeType.Element) continue;
            if (!child.Name.Equals("li", StringComparison.OrdinalIgnoreCase)) continue;

            counter++;
            sb.Append(CultureInfo.InvariantCulture, $"{counter}. ");
            ProcessNode(child, sb, links);
            sb.AppendLine();
        }
    }

    private void RenderTable(HtmlNode table, StringBuilder sb, List<ExtractedLink> links)
    {
        var headerRow = table.SelectSingleNode(".//thead/tr")
                        ?? table.SelectSingleNode(".//tr[th]");

        var bodyRows = table.SelectNodes(".//tbody/tr")
                       ?? table.SelectNodes(".//tr[td]");

        if (headerRow is not null)
        {
            var headerCells = headerRow.SelectNodes("th|td");
            if (headerCells is not null)
            {
                RenderTableRow(headerCells, sb, links);

                sb.Append('|');
                for (var i = 0; i < headerCells.Count; i++)
                    sb.Append(" --- |");
                sb.AppendLine();
            }
        }

        if (bodyRows is null) return;

        foreach (var row in bodyRows)
        {
            if (row == headerRow) continue;

            var cells = row.SelectNodes("th|td");
            if (cells is not null)
                RenderTableRow(cells, sb, links);
        }
    }

    private void RenderTableRow(HtmlNodeCollection cells, StringBuilder sb, List<ExtractedLink> links)
    {
        sb.Append('|');
        foreach (var cell in cells)
        {
            var cellSb = new StringBuilder();
            ProcessNode(cell, cellSb, links);
            var text = cellSb.ToString()
                .Trim()
                .Replace("|", "\\|", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal);
            sb.Append(' ').Append(text).Append(" |");
        }

        sb.AppendLine();
    }

    private sealed record ParsedUrl(bool IsInternal, string? Lang, string? ExternalRef, string? FullUrl);

    private static ParsedUrl? ParseUrl(string href)
    {
        var url = href.Trim();

        var hash = url.IndexOf('#');
        if (hash >= 0)
            url = url[..hash];
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (url.StartsWith(MdnConstants.BaseUrl, StringComparison.OrdinalIgnoreCase))
            url = new Uri(url).AbsolutePath;

        var docsIdx = url.IndexOf("/docs/", StringComparison.OrdinalIgnoreCase);
        if (docsIdx >= 0)
        {
            var prefix = url[..docsIdx];
            var parts = prefix.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var langRaw = parts.LastOrDefault() ?? "en-US";
            var lang = LanguageHelpers.NormalizeLang(langRaw);
            var externalRef = url[(docsIdx + "/docs/".Length)..].Trim('/');

            return string.IsNullOrWhiteSpace(externalRef)
                ? null
                : new ParsedUrl(true, lang, externalRef, null);
        }

        // Relative non-docs URL (e.g. /media/...) — resolve against MDN base
        if (url.StartsWith('/'))
        {
            var absolute = MdnConstants.BaseUrl + url;
            return new ParsedUrl(false, null, null, absolute);
        }

        return Uri.TryCreate(href, UriKind.Absolute, out _)
            ? new ParsedUrl(false, null, null, href)
            : null;
    }

    /// <summary>
    /// Extracts the programming language identifier for a code fence.
    /// Checks the &lt;code&gt; element's class first (e.g. <c>language-js</c>),
    /// then falls back to the parent &lt;pre&gt;'s <c>brush: js</c> class
    /// used by MDN's modern code-example format.
    /// </summary>
    private static string ExtractCodeLang(HtmlNode codeNode)
    {
        var lang = LangFromClass(codeNode.GetAttributeValue("class", ""));
        if (!string.IsNullOrEmpty(lang)) return lang;

        var pre = codeNode.ParentNode;
        if (pre?.Name.Equals("pre", StringComparison.OrdinalIgnoreCase) == true)
            return ExtractLangFromPre(pre);

        return "";
    }

    /// <summary>
    /// Extracts the language from a &lt;pre class="brush: js …"&gt; element.
    /// </summary>
    private static string ExtractLangFromPre(HtmlNode preNode)
    {
        var m = BrushLang.Match(preNode.GetAttributeValue("class", ""));
        return m.Success && KnownCodeLangs.Contains(m.Groups[1].Value)
            ? m.Groups[1].Value.ToLowerInvariant()
            : "";
    }

    /// <summary>
    /// Scans a CSS class string for a known language token.
    /// Handles both <c>language-js</c> (Prism convention) and bare tokens like <c>js</c>.
    /// </summary>
    private static string LangFromClass(string cls)
    {
        foreach (var part in cls.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.StartsWith("language-", StringComparison.OrdinalIgnoreCase))
                return part["language-".Length..];

            if (KnownCodeLangs.Contains(part))
                return part;
        }

        return "";
    }

    /// <summary>Returns true when the element has the given CSS class.</summary>
    private static bool HasClass(HtmlNode node, string className)
        => node.GetAttributeValue("class", "")
               .Split(' ', StringSplitOptions.RemoveEmptyEntries)
               .Any(c => c.Equals(className, StringComparison.OrdinalIgnoreCase));

    private static string GetPlainText(HtmlNode node)
        => WebUtility.HtmlDecode(node.InnerText).Trim();

    private static string NormalizeText(StringBuilder sb)
        => MultipleNewlines.Replace(sb.ToString(), "\n\n").Trim();
}
