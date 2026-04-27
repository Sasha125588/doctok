using Ganss.Xss;
using Markdig;

namespace Infrastructure.PostGeneration;

/// <summary>
/// Converts a Markdown string to a sanitized HTML fragment safe for v-html.
/// Thread-safe — pipeline and sanitizer are built once and reused.
/// </summary>
public sealed class MarkdownHtmlRenderer
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAutoLinks()
        .UsePipeTables()
        .UseEmphasisExtras()
        .UseListExtras()
        .UseGenericAttributes()
        .Build();

    private readonly HtmlSanitizer _sanitizer = CreateSanitizer();

    public string Render(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var html = Markdown.ToHtml(markdown, _pipeline).Trim();
        return _sanitizer.Sanitize(html).Trim();
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedSchemes.Add("mdn");
        sanitizer.AllowedAttributes.Add("class");
        return sanitizer;
    }
}
