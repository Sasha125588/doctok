using Infrastructure.PostGeneration;
using Xunit;

namespace Api.Tests.Infrastructure;

public sealed class FastPostGeneratorTests
{
    private readonly FastPostGenerator _gen = new();

    // ─── See-also section skip ───────────────────────────────────────────────

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

    // ─── Fully-italic preamble filter ────────────────────────────────────────

    [Theory]
    [InlineData("*Inherits properties from its parent, [`CharacterData`](mdn/characterdata).*")]
    [InlineData("*Интерфейс `Text` включает следующее свойство, определяемое при смешивании [`Slotable`](mdn/slotable).*")]
    [InlineData("*Наследует родительские методы, [`CharacterData`](mdn/characterdata).*")]
    public void FullyItalicSingleLinePreambleIsFiltered(string body)
    {
        var md = $"""
            ## Properties

            {body}

            **[`SomeMethod()`](mdn/somemethod)**
            : Does something.
            """;

        var posts = _gen.Generate(md);

        // The preamble must not produce a post
        Assert.DoesNotContain(posts, p => p.Body == body);
    }

    [Fact]
    public void ParagraphWithPartialItalicIsNotFiltered()
    {
        // A paragraph that contains italic but is not FULLY italic must pass through
        const string body = "Use *this* method when you need async behavior.";
        var md = $"""
            ## Description

            {body}
            """;

        var posts = _gen.Generate(md);

        Assert.Contains(posts, p => p.Body.Contains("Use"));
    }

    [Fact]
    public void BoldTextIsNotFilteredByItalicCheck()
    {
        // **bold** starts with * but is double-asterisk, must NOT be filtered
        const string body = "**Important:** this method modifies the node in place.";
        var md = $"""
            ## Description

            {body}
            """;

        var posts = _gen.Generate(md);

        Assert.Contains(posts, p => p.Body.Contains("Important"));
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

        var summary = Assert.Single(posts, p => p.Kind == "summary");
        Assert.Contains("Fetch API", summary.Body);
    }

    // ─── Definition list style blocks (dt+dd pairs) ──────────────────────────

    [Fact]
    public void DtDdBlockBecomesASingleFactPost()
    {
        // After MdnContentConverter's RenderDefinitionList, the format is:
        // **term**\n: definition   (no blank line between them)
        var md = """
            ## Constructor

            **[`Text()`](mdn/web/api/text/text)**
            : Returns a Text node with the given content.
            """;

        var posts = _gen.Generate(md);

        var fact = Assert.Single(posts, p => p.Kind == "fact");
        Assert.Contains("Text()", fact.Body);
        Assert.Contains("Returns a Text node", fact.Body);
    }

    [Fact]
    public void MultipleDtDdPairsInOneBlockFormOneFactPost()
    {
        // All pairs from a single <dl> arrive without blank lines between them
        var md = """
            ## Properties

            **[`Text.wholeText`](mdn/web/api/text/wholetext) Read only**
            : Returns a string with all adjacent text.
            **[`Text.splitText`](mdn/web/api/text/splittext)**
            : Splits the node at the given offset.
            """;

        var posts = _gen.Generate(md);

        // Both properties in ONE post, not two
        var fact = Assert.Single(posts, p => p.Kind == "fact");
        Assert.Contains("wholeText", fact.Body);
        Assert.Contains("splitText", fact.Body);
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

        var example = Assert.Single(posts, p => p.Kind == "example");
        Assert.Contains("fetch", example.Body);
    }
}
