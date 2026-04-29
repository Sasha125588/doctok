using Domain.Mdn;
using Domain.Posts;
using Infrastructure.PostGeneration.Fast;
using Xunit;

namespace Api.Tests.Infrastructure;

public sealed class FastPostGeneratorTests
{
    private readonly FastPostGenerator _gen = new();

    // ─── Skip rules ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("See also")]
    [InlineData("See Also")]
    [InlineData("Browser compatibility")]
    [InlineData("Specifications")]
    [InlineData("Смотрите также")]
    [InlineData("Совместимость с браузерами")]
    public void SeeAlsoAndCompatSectionsAreSkipped(string heading)
    {
        var sections = new MdnSection[]
        {
            new("intro", "Intro", false, "A useful paragraph about the topic."),
            new("compat", heading, false, "Some content here."),
        };

        var posts = _gen.Generate(sections);

        Assert.DoesNotContain(posts, p => p.SourceSectionKey == "compat");
    }

    // ─── Summary extraction ──────────────────────────────────────────────────

    [Fact]
    public void FirstSectionBecomesASummaryPost()
    {
        var sections = new MdnSection[]
        {
            new(null, null, false, "The Fetch API provides an interface for fetching resources across the network."),
            new("concepts", "Concepts", false, "Some concepts here about how it works."),
        };

        var posts = _gen.Generate(sections);

        var summary = Assert.Single(posts, p => p.Kind == PostKind.Summary);
        Assert.Contains("Fetch API", summary.Body);
    }

    // ─── Concept sections ────────────────────────────────────────────────────

    [Fact]
    public void H2SectionBecomesConceptPost()
    {
        var sections = new MdnSection[]
        {
            new(null, null, false, "Lead paragraph."),
            new("constructor", "Constructor", false, "**Text()** : Returns a Text node."),
        };

        var posts = _gen.Generate(sections);

        var concept = Assert.Single(posts, p => p.Kind == PostKind.Concept);
        Assert.Contains("Text()", concept.Body);
        Assert.Contains("Returns a Text node", concept.Body);
    }

    [Fact]
    public void H3SectionsAttachToPreviousH2()
    {
        var sections = new MdnSection[]
        {
            new("intro", "Introduction", false, "Intro body"),
            new("details", "Details", true, "Nested body"),
        };

        var posts = _gen.Generate(sections);

        var post = Assert.Single(posts);
        Assert.Equal("intro", post.SourceSectionKey);
        Assert.Contains("Intro body", post.Body);
        Assert.Contains("### Details", post.Body);
        Assert.Contains("Nested body", post.Body);
    }

    [Fact]
    public void MultipleH3SubsectionsAttachToParentH2()
    {
        var sections = new MdnSection[]
        {
            new("properties", "Properties", false, "Top-level properties text."),
            new("deprecated", "Deprecated properties", true, "splitText is deprecated."),
        };

        var posts = _gen.Generate(sections);

        var concept = Assert.Single(posts, p => p.Kind == PostKind.Concept);
        Assert.Contains("Top-level properties text.", concept.Body);
        Assert.Contains("splitText is deprecated.", concept.Body);
    }

    // ─── Example section ─────────────────────────────────────────────────────

    [Fact]
    public void SectionTitledExampleClassifiesAsExample()
    {
        var sections = new MdnSection[]
        {
            new(null, null, false, "Lead."),
            new("examples", "Examples", false, "Fetch a resource: code here."),
        };

        var posts = _gen.Generate(sections);

        var example = Assert.Single(posts, p => p.Kind == PostKind.Example);
        Assert.Contains("Fetch a resource", example.Body);
    }

    // ─── Empty sections ──────────────────────────────────────────────────────

    [Fact]
    public void EmptySectionsAreSkipped()
    {
        var sections = new MdnSection[]
        {
            new("concepts", "Concepts and usage", false, ""),
            new("description", "Description", false, "Something useful here."),
        };

        var posts = _gen.Generate(sections);

        Assert.Single(posts);
        Assert.Contains("Something useful", posts[0].Body);
    }

    // ─── Source section keys ─────────────────────────────────────────────────

    [Fact]
    public void SectionWithoutIdGetsDeterministicFallbackKey()
    {
        var sections = new MdnSection[]
        {
            new(null, null, false, "Intro."),
            new(null, "Description", false, "Body."),
        };

        var posts = _gen.Generate(sections);

        Assert.Equal("section-0", posts[0].SourceSectionKey);
        Assert.Equal("section-1", posts[1].SourceSectionKey);
    }

    [Fact]
    public void SectionIdIsLowercasedForKey()
    {
        var sections = new MdnSection[]
        {
            new("Intro", null, false, "Hello."),
        };

        var posts = _gen.Generate(sections);

        Assert.Equal("intro", Assert.Single(posts).SourceSectionKey);
    }
}
