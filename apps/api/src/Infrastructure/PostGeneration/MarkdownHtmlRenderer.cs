using Ganss.Xss;
using Markdig;

namespace Infrastructure.PostGeneration;

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
