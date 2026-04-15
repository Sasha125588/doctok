using Markdig;

namespace Infrastructure.PostGeneration;

/// <summary>
/// Converts a Markdown string to an HTML fragment using Markdig.
/// Thread-safe — the <see cref="MarkdownPipeline"/> is built once and reused.
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

    public string Render(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        return Markdown.ToHtml(markdown, _pipeline).Trim();
    }
}
