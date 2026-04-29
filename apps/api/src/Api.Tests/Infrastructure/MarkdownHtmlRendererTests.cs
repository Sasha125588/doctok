using Infrastructure.PostGeneration;
using Xunit;

namespace Api.Tests.Infrastructure;

public sealed class MarkdownHtmlRendererTests
{
    [Fact]
    public void RenderStripsScriptTags()
    {
        var renderer = new MarkdownHtmlRenderer();

        var html = renderer.Render("<script>alert('x')</script>safe");

        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("safe", html);
    }

    [Fact]
    public void RenderStripsEventHandlers()
    {
        var renderer = new MarkdownHtmlRenderer();

        var html = renderer.Render("""<a href="https://example.com" onclick="alert(1)">link</a>""");

        Assert.DoesNotContain("onclick", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"https://example.com\"", html);
    }

    [Fact]
    public void RenderAllowsInternalTopicLinks()
    {
        var renderer = new MarkdownHtmlRenderer();

        var html = renderer.Render("[Fetch](mdn/web/api/fetch_api)");

        Assert.Contains("href=\"mdn/web/api/fetch_api\"", html);
    }
}
