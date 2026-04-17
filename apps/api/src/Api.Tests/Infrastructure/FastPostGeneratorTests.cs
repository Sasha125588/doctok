using Domain.Posts;
using Infrastructure.PostGeneration.Fast;
using Xunit;

namespace Api.Tests.Infrastructure;

public sealed class FastPostGeneratorTests
{
    private readonly FastPostGenerator _gen = new();

    // ─── See-also / compat section skip ──────────────────────────────────────

    [Theory]
    [InlineData("## See also")]
    [InlineData("## See Also")]
    [InlineData("## Browser compatibility")]
    [InlineData("## Specifications")]
    [InlineData("## Смотрите также")]
    [InlineData("## Смотри(те) также")]
    [InlineData("## Смотри также")]
    [InlineData("## см. также")]
    [InlineData("## Совместимость с браузерами")]
    public void SeeAlsoAndCompatSectionsAreSkipped(string heading)
    {
        var md = $"""
            ## Intro

            A useful paragraph about the topic.

            {heading}

            - [Some link](mdn/something).
            """;

        var posts = _gen.Generate(md);

        Assert.DoesNotContain(posts, p => p.Body.Contains("Some link"));
    }

    // ─── Summary extraction ──────────────────────────────────────────────────

    [Fact]
    public void FirstParagraphBecomesASummaryPost()
    {
        var md = """
            The Fetch API provides an interface for fetching resources across the network.

            ## Concepts

            Some concepts here about how it works.
            """;

        var posts = _gen.Generate(md);

        var summary = Assert.Single(posts, p => p.Kind.ToStorageValue() == "summary");
        Assert.Contains("Fetch API", summary.Body);
    }

    // ─── Concept sections ────────────────────────────────────────────────────

    [Fact]
    public void H2SectionBecomesConceptPost()
    {
        var md = """
            ## Constructor

            **[`Text()`](mdn/web/api/text/text)**
            : Returns a Text node with the given content.
            """;

        var posts = _gen.Generate(md);

        var concept = Assert.Single(posts, p => p.Kind.ToStorageValue() == "concept");
        Assert.Contains("Text()", concept.Body);
        Assert.Contains("Returns a Text node", concept.Body);
    }

    [Fact]
    public void H3SubsectionsStayWithParentH2()
    {
        var md = """
            ## Properties

            **[`Text.wholeText`](mdn/web/api/text/wholetext) Read only**
            : Returns a string with all adjacent text.

            ### Deprecated properties

            **[`Text.splitText`](mdn/web/api/text/splittext)**
            : Splits the node at the given offset.
            """;

        var posts = _gen.Generate(md);

        // Both H2 content and H3 subsection in ONE post
        var concept = Assert.Single(posts, p => p.Kind.ToStorageValue() == "concept");
        Assert.Contains("wholeText", concept.Body);
        Assert.Contains("splitText", concept.Body);
    }

    // ─── Example section ─────────────────────────────────────────────────────

    [Fact]
    public void CodeBlockInExampleSectionBecomesExample()
    {
        var md = """
            ## Example

            Fetch a resource:

            ```js
            const response = await fetch('/api/data');
            const json = await response.json();
            ```
            """;

        var posts = _gen.Generate(md);

        var example = Assert.Single(posts, p => p.Kind.ToStorageValue() == "example");
        Assert.Contains("fetch", example.Body);
    }

    // ─── Empty / whitespace sections ─────────────────────────────────────────

    [Fact]
    public void EmptySectionsAreSkipped()
    {
        var md = """
            ## Concepts and usage



            ## Description

            Something useful here.
            """;

        var posts = _gen.Generate(md);

        Assert.Single(posts);
        Assert.Contains("Something useful", posts[0].Body);
    }
}
