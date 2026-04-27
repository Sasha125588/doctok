using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Mdn;

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

public sealed class MdnContentConverter(MdnMarkdownConverter markdownConverter)
{
    private static readonly Regex MultipleNewlines = new(@"\n{3,}", RegexOptions.Compiled);

    public (string Text, IReadOnlyList<ExtractedLink> Links) Convert(MdnDocument doc)
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

            var converted = markdownConverter.ConvertHtml(section.Content);
            links.AddRange(converted.Links);

            if (!string.IsNullOrEmpty(converted.Markdown))
            {
                sb.AppendLine(converted.Markdown);
                sb.AppendLine();
            }
        }

        var text = MultipleNewlines.Replace(sb.ToString(), "\n\n").Trim();

        var distinct = links
            .DistinctBy(l => (l.Kind, l.TargetLang, l.TargetExternalRef, l.Url))
            .ToList();

        return (text, distinct);
    }
}
