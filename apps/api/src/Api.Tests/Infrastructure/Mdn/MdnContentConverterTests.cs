using Infrastructure.Sources.Mdn;
using Xunit;

namespace Api.Tests.Infrastructure.Mdn;

public sealed class MdnContentConverterTests
{
    private static readonly string[] OtherLocales = ["ru", "fr"];
    private readonly MdnContentConverter _converter = new();

    private static MdnApiDoc MakeDoc(params MdnApiSection[] sections)
        => new("Test", "Web/API/Test", sections, null, null, null, Array.Empty<string>());

    // ─── Basic prose conversion ──────────────────────────────────────

    [Fact]
    public void SimpleParagraphConvertsToPlainText()
    {
        var doc = MakeDoc(new MdnApiSection(null, null, false, "<p>Hello world</p>"));
        var (text, _) = _converter.Convert(doc);

        Assert.Equal("Hello world", text);
    }

    [Fact]
    public void SectionTitleRendersAsH2()
    {
        var doc = MakeDoc(new MdnApiSection("intro", "Introduction", false, "<p>Content here</p>"));
        var (text, _) = _converter.Convert(doc);

        Assert.StartsWith("## Introduction", text);
        Assert.Contains("Content here", text);
    }

    [Fact]
    public void IsH3SectionRendersAsH3()
    {
        var doc = MakeDoc(new MdnApiSection("sub", "Sub Section", true, "<p>Body</p>"));
        var (text, _) = _converter.Convert(doc);

        Assert.StartsWith("### Sub Section", text);
    }

    [Fact]
    public void EmptyContentSectionRendersHeadingOnly()
    {
        var doc = MakeDoc(new MdnApiSection("empty", "Empty Section", false, ""));
        var (text, _) = _converter.Convert(doc);

        Assert.Equal("## Empty Section", text);
    }

    // ─── Inline formatting ──────────────────────────────────────────

    [Fact]
    public void InlineCodeRendersWithBackticks()
    {
        var doc = MakeDoc(new MdnApiSection(null, null, false, "<p>Use <code>fetch()</code> here</p>"));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("`fetch()`", text);
    }

    [Fact]
    public void StrongRendersWithDoubleAsterisks()
    {
        var doc = MakeDoc(new MdnApiSection(null, null, false, "<p><strong>bold</strong> text</p>"));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("**bold**", text);
    }

    [Fact]
    public void EmRendersWithSingleAsterisk()
    {
        var doc = MakeDoc(new MdnApiSection(null, null, false, "<p><em>italic</em> text</p>"));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("*italic*", text);
    }

    // ─── Lists ──────────────────────────────────────────────────────

    [Fact]
    public void UnorderedListRendersWithDashes()
    {
        var html = "<ul><li>One</li><li>Two</li></ul>";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("- One", text);
        Assert.Contains("- Two", text);
    }

    [Fact]
    public void OrderedListRendersWithNumbers()
    {
        var html = "<ol><li>First</li><li>Second</li></ol>";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("1. First", text);
        Assert.Contains("2. Second", text);
    }

    [Fact]
    public void ParagraphInsideLiDoesNotProduceDoubleNewlines()
    {
        var html = "<ul><li><p>Item one</p></li><li><p>Item two</p></li></ul>";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        // Should not have triple+ newlines (NormalizeText collapses them,
        // but without the fix the raw output would have them).
        Assert.DoesNotContain("\n\n\n", text);
        Assert.Contains("- Item one", text);
        Assert.Contains("- Item two", text);
    }

    // ─── Definition lists ───────────────────────────────────────────

    [Fact]
    public void DefinitionListRendersCorrectly()
    {
        var html = "<dl><dt>Term</dt><dd><p>Definition text</p></dd></dl>";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("**Term**", text);
        Assert.Contains(": Definition text", text);
    }

    [Fact]
    public void ParagraphInsideDdDoesNotProduceDoubleNewlines()
    {
        var html = "<dl><dt>T1</dt><dd><p>D1</p></dd><dt>T2</dt><dd><p>D2</p></dd></dl>";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.DoesNotContain("\n\n\n", text);
    }

    /// <summary>
    /// The primary regression for posts #48/#49 in the Text API page.
    /// A &lt;dt&gt; and its &lt;dd&gt; must end up in the same text block —
    /// no blank line between them — so FastPostGenerator keeps them as one post.
    /// </summary>
    [Fact]
    public void DtAndDdAreNotSeparatedByBlankLine()
    {
        // Realistic MDN HTML: whitespace text nodes between dt and dd
        var html = """
            <dl>
            <dt id="foo"><code>foo()</code></dt>
            <dd>
            <p>Does something useful.</p>
            </dd>
            </dl>
            """;
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("**`foo()`**", text);
        Assert.Contains(": Does something useful.", text);

        // The dt and dd must NOT be separated by a blank line
        var dtIndex = text.IndexOf("**`foo()`**", StringComparison.Ordinal);
        var ddIndex = text.IndexOf(": Does something useful.", StringComparison.Ordinal);
        var between = text[dtIndex..ddIndex];
        Assert.DoesNotContain("\n\n", between);
    }

    [Fact]
    public void MultipleDtDdPairsFormOneContiguousBlock()
    {
        // All pairs in a <dl> should be one block — no blank lines between pairs
        var html = """
            <dl>
            <dt>Alpha</dt>
            <dd><p>First definition.</p></dd>
            <dt>Beta</dt>
            <dd><p>Second definition.</p></dd>
            <dt>Gamma</dt>
            <dd><p>Third definition.</p></dd>
            </dl>
            """;
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("**Alpha**", text);
        Assert.Contains("**Beta**", text);
        Assert.Contains("**Gamma**", text);

        // No blank line anywhere between Alpha and Gamma blocks
        var start = text.IndexOf("**Alpha**", StringComparison.Ordinal);
        var end = text.IndexOf(": Third definition.", StringComparison.Ordinal)
                  + ": Third definition.".Length;
        var block = text[start..end];
        Assert.DoesNotContain("\n\n", block);
    }

    [Fact]
    public void VisuallyHiddenSpansAreSkipped()
    {
        var html = """
            <dl>
            <dt>
              <code>Text()</code>
              <abbr class="icon icon-experimental" title="Experimental">
                <span class="visually-hidden">Experimental</span>
              </abbr>
            </dt>
            <dd><p>Creates a Text node.</p></dd>
            </dl>
            """;
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        // Screen-reader text must not appear in the output
        Assert.DoesNotContain("Experimental", text);
        Assert.DoesNotContain("visually-hidden", text);

        // The term and definition must still be present and together
        Assert.Contains("**`Text()`**", text);
        Assert.Contains(": Creates a Text node.", text);
    }

    [Fact]
    public void MdnApiStyleDtWithReadOnlyBadge()
    {
        // Mirrors the real MDN "Properties" section HTML
        var html = """
            <dl>
            <dt>
              <a href="/en-US/docs/Web/API/Text/wholeText"><code>Text.wholeText</code></a>
              <span class="badge inline readonly">Read only </span>
            </dt>
            <dd>
              <p>Returns a string with all adjacent text.</p>
            </dd>
            </dl>
            """;
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        // Badge text IS useful (unlike visually-hidden), so it must appear
        Assert.Contains("Read only", text);
        Assert.Contains("[`Text.wholeText`]", text);
        Assert.Contains(": Returns a string with all adjacent text.", text);

        // No blank line between dt and dd
        var dtIdx = text.IndexOf("**[`Text.wholeText`]", StringComparison.Ordinal);
        var ddIdx = text.IndexOf(": Returns a string", StringComparison.Ordinal);
        Assert.DoesNotContain("\n\n", text[dtIdx..ddIdx]);
    }

    // ─── Code blocks ────────────────────────────────────────────────

    [Fact]
    public void PreCodeBlockRendersAsFencedCode()
    {
        var html = """<pre><code class="language-js">console.log("hi");</code></pre>""";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("```js", text);
        Assert.Contains("console.log(\"hi\");", text);
        Assert.Contains("```", text);
    }

    [Fact]
    public void PreWithoutCodeRendersAsFencedBlock()
    {
        var html = "<pre>plain preformatted</pre>";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("```\nplain preformatted\n```", text);
    }

    // ─── Links ──────────────────────────────────────────────────────

    [Fact]
    public void InternalMdnLinkExtractsSlugAndLang()
    {
        var html = """<p><a href="/en-US/docs/Web/API/Fetch_API">Fetch API</a></p>""";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, links) = _converter.Convert(doc);

        Assert.Contains("[Fetch API](mdn/web/api/fetch_api)", text);

        var link = Assert.Single(links, l => l.Kind == "internal");
        Assert.Equal("en", link.TargetLang);
        Assert.Equal("Web/API/Fetch_API", link.TargetExternalRef);
        Assert.Equal("Fetch API", link.Label);
    }

    [Fact]
    public void InternalLinkWithLocalizedPathExtractsCorrectLang()
    {
        var html = """<p><a href="/ru/docs/Web/API/SpeechRecognition">SpeechRecognition</a></p>""";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (_, links) = _converter.Convert(doc);

        var link = Assert.Single(links, l => l.Kind == "internal");
        Assert.Equal("ru", link.TargetLang);
    }

    [Fact]
    public void ExternalLinkIsPreserved()
    {
        var html = """<p><a href="https://www.w3.org/TR/jsgf/" class="external">JSGF</a></p>""";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, links) = _converter.Convert(doc);

        Assert.Contains("[JSGF](https://www.w3.org/TR/jsgf/)", text);

        var link = Assert.Single(links, l => l.Kind == "external");
        Assert.Equal("https://www.w3.org/TR/jsgf/", link.Url);
    }

    [Fact]
    public void HashOnlyLinkRendersAsPlainText()
    {
        var html = """<p><a href="#section">Jump</a></p>""";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, links) = _converter.Convert(doc);

        Assert.Contains("Jump", text);
        Assert.DoesNotContain("[Jump]", text);
        Assert.Empty(links);
    }

    [Fact]
    public void LinkWithHashFragmentStripsFragment()
    {
        var html = """<p><a href="/en-US/docs/Web/API/Fetch_API#examples">Fetch examples</a></p>""";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (_, links) = _converter.Convert(doc);

        var link = Assert.Single(links, l => l.Kind == "internal");
        Assert.Equal("Web/API/Fetch_API", link.TargetExternalRef);
    }

    [Fact]
    public void RelativeNonDocsUrlResolvesAgainstMdnBase()
    {
        var html = """<p><a href="/media/diagram.png">Diagram</a></p>""";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, links) = _converter.Convert(doc);

        Assert.Contains("[Diagram](https://developer.mozilla.org/media/diagram.png)", text);
        var link = Assert.Single(links, l => l.Kind == "external");
        Assert.Equal("https://developer.mozilla.org/media/diagram.png", link.Url);
    }

    [Fact]
    public void AnchorWithoutHrefRendersPlainText()
    {
        var html = "<p><a>no href</a></p>";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("no href", text);
        Assert.DoesNotContain("[no href]", text);
    }

    // ─── Tables ─────────────────────────────────────────────────────

    [Fact]
    public void SimpleTableRendersAsMarkdownTable()
    {
        var html = "<table><thead><tr><th>Header</th></tr></thead><tbody><tr><td>Cell</td></tr></tbody></table>";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("| Header |", text);
        Assert.Contains("| --- |", text);
        Assert.Contains("| Cell |", text);
    }

    // ─── Deduplication ──────────────────────────────────────────────

    [Fact]
    public void DuplicateLinksAreDeduped()
    {
        var html = """
            <p>
                <a href="/en-US/docs/Web/API/Fetch_API">Fetch</a> and
                <a href="/en-US/docs/Web/API/Fetch_API">Fetch again</a>
            </p>
            """;
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (_, links) = _converter.Convert(doc);

        Assert.Single(links, l => l.Kind == "internal");
    }

    // ─── Multi-section (real-world-like) ────────────────────────────

    [Fact]
    public void MultipleSectionsProduceCoherentOutput()
    {
        var sections = new MdnApiSection[]
        {
            new(null, null, false, "<p>Web Speech API enables voice interfaces.</p>"),
            new("concepts", "Concepts", false, "<p>Two components: <a href=\"/en-US/docs/Web/API/SpeechRecognition\"><code>SpeechRecognition</code></a> and synthesis.</p>"),
            new("interfaces", "Interfaces", false, ""),
            new("recognition", "Speech Recognition", true, "<dl><dt><a href=\"/en-US/docs/Web/API/SpeechRecognition\"><code>SpeechRecognition</code></a></dt><dd><p>Controller interface.</p></dd></dl>"),
        };
        var doc = new MdnApiDoc(
            "Web Speech API",
            "Web/API/Web_Speech_API",
            sections,
            "web-api-overview",
            null,
            null,
            OtherLocales);

        var (text, links) = _converter.Convert(doc);

        Assert.Contains("Web Speech API enables voice interfaces.", text);
        Assert.Contains("## Concepts", text);
        Assert.Contains("## Interfaces", text);
        Assert.Contains("### Speech Recognition", text);
        Assert.Contains("**[`SpeechRecognition`](mdn/web/api/speechrecognition)**", text);
        Assert.Contains(": Controller interface.", text);

        // Only one unique internal link despite two references
        Assert.Single(links, l => l.Kind == "internal");
    }

    // ─── Normalization ──────────────────────────────────────────────

    [Fact]
    public void TripleNewlinesAreCollapsedToDouble()
    {
        var doc = MakeDoc(
            new MdnApiSection(null, null, false, "<p>Before</p>"),
            new MdnApiSection("s", "Section", false, ""),
            new MdnApiSection(null, null, false, "<p>After</p>"));

        var (text, _) = _converter.Convert(doc);

        Assert.DoesNotContain("\n\n\n", text);
    }

    // ─── HTML entities ──────────────────────────────────────────────

    [Fact]
    public void HtmlEntitiesAreDecoded()
    {
        var doc = MakeDoc(new MdnApiSection(null, null, false, "<p>A &amp; B &lt; C</p>"));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("A & B < C", text);
    }

    // ─── Image alt text ─────────────────────────────────────────────

    [Fact]
    public void ImageAltTextIsRendered()
    {
        var doc = MakeDoc(new MdnApiSection(
            null,
            null,
            false,
            """<p><img alt="Diagram of the API" src="/img.png" /></p>"""));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("Diagram of the API", text);
    }

    // ─── MDN code-example wrapper (brush: lang) ──────────────────────

    /// <summary>
    /// MDN wraps code blocks in:
    ///   &lt;div class="code-example"&gt;
    ///     &lt;div class="example-header"&gt;&lt;span class="language-name"&gt;js&lt;/span&gt;&lt;/div&gt;
    ///     &lt;pre class="brush: js notranslate"&gt;&lt;code&gt;…&lt;/code&gt;&lt;/pre&gt;
    ///   &lt;/div&gt;
    /// The "example-header" must be silently skipped; the language must come from
    /// the "brush:" class on &lt;pre&gt;, not leak as plain text before the fence.
    /// </summary>
    [Fact]
    public void MdnCodeExampleWrapperRendersCorrectly()
    {
        var html = """
            <div class="code-example">
              <div class="example-header"><span class="language-name">js</span></div>
              <pre class="brush: js notranslate"><code>const x = 1;</code></pre>
            </div>
            """;
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("```js", text);
        Assert.Contains("const x = 1;", text);

        // "js" must NOT appear as a bare word before the opening fence
        Assert.DoesNotContain("js\n```", text);
        Assert.DoesNotContain("js```", text);
        Assert.DoesNotContain("language-name", text);
    }

    [Fact]
    public void BrushClassOnPreIsUsedForLanguage()
    {
        // When <code> has no class, the language must come from <pre class="brush: css …">
        var html = """<pre class="brush: css notranslate"><code>body { margin: 0; }</code></pre>""";
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        Assert.Contains("```css", text);
        Assert.Contains("body { margin: 0; }", text);
    }

    [Fact]
    public void MultipleConsecutiveMdnCodeBlocksAllRenderCorrectly()
    {
        // Regression: previously each example-header leaked "js" which confused
        // SplitPreservingCodeFences and produced broken / truncated posts.
        var html = """
            <p>First example:</p>
            <div class="code-example">
              <div class="example-header"><span class="language-name">js</span></div>
              <pre class="brush: js notranslate"><code>const a = 1;</code></pre>
            </div>
            <p>Second example:</p>
            <div class="code-example">
              <div class="example-header"><span class="language-name">js</span></div>
              <pre class="brush: js notranslate"><code>const b = 2;</code></pre>
            </div>
            <p>Third example:</p>
            <div class="code-example">
              <div class="example-header"><span class="language-name">js</span></div>
              <pre class="brush: js notranslate"><code>const c = 3;</code></pre>
            </div>
            """;
        var doc = MakeDoc(new MdnApiSection(null, null, false, html));
        var (text, _) = _converter.Convert(doc);

        // Every block must open with ```js (not js```)
        var fenceCount = text.Split("```js").Length - 1;
        Assert.Equal(3, fenceCount);

        Assert.Contains("const a = 1;", text);
        Assert.Contains("const b = 2;", text);
        Assert.Contains("const c = 3;", text);
        Assert.DoesNotContain("js\n```", text);
        Assert.DoesNotContain("js```", text);
    }
}
