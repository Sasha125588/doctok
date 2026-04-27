using System.Net;
using Domain.Mdn;
using Domain.Shared;
using Domain.Sources;
using HtmlAgilityPack;
using ReverseMarkdown;

namespace Infrastructure.Sources.Mdn;

public sealed class MdnMarkdownConverter
{
    private readonly Converter _converter = new(new Config
    {
        GithubFlavored = true,
        RemoveComments = true,
        SmartHrefHandling = true,
        UnknownTags = Config.UnknownTagsOption.Bypass,
    });

    public (string Markdown, IReadOnlyList<ExtractedLink> Links) ConvertHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return (string.Empty, []);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        RemoveNodes(doc, "//*[contains(concat(' ', normalize-space(@class), ' '), ' example-header ')]");
        RemoveNodes(doc, "//*[contains(concat(' ', normalize-space(@class), ' '), ' visually-hidden ')]");

        var links = RewriteLinks(doc);
        var markdown = _converter.Convert(doc.DocumentNode.OuterHtml).Trim();

        return (markdown, links);
    }

    private static void RemoveNodes(HtmlDocument doc, string xpath)
    {
        foreach (var node in doc.DocumentNode.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>())
            node.Remove();
    }

    private static IReadOnlyList<ExtractedLink> RewriteLinks(HtmlDocument doc)
    {
        var result = new List<ExtractedLink>();

        foreach (var anchor in doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
        {
            var href = anchor.GetAttributeValue("href", "");
            var label = WebUtility.HtmlDecode(anchor.InnerText).Trim();
            var parsed = ParseUrl(href);

            if (parsed is null)
                continue;

            if (parsed.IsInternal)
            {
                var slug = parsed.ExternalRef!.ToLowerInvariant();
                anchor.SetAttributeValue("href", $"{SourceCodes.Mdn}/{slug}");
                result.Add(new ExtractedLink("internal", parsed.Lang, parsed.ExternalRef, null, label));
            }
            else
            {
                anchor.SetAttributeValue("href", parsed.FullUrl ?? string.Empty);
                result.Add(new ExtractedLink("external", null, null, parsed.FullUrl, label));
            }
        }

        return result
            .DistinctBy(x => (x.Kind, x.TargetLang, x.TargetExternalRef, x.Url))
            .ToList();
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

        if (url.StartsWith('/'))
        {
            var absolute = MdnConstants.BaseUrl + url;
            return new ParsedUrl(false, null, null, absolute);
        }

        return Uri.TryCreate(href, UriKind.Absolute, out _)
            ? new ParsedUrl(false, null, null, href)
            : null;
    }
}
