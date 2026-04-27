using System.Globalization;
using System.Text;
using Domain.Mdn;
using Domain.Posts;

namespace Infrastructure.PostGeneration.Fast;

/// <summary>
/// Generates original posts from MDN sections.
/// Each H2 section becomes one post; H3 sections attach to the preceding H2.
/// Skipped sections (browser compat, specifications, see also) are not emitted.
/// </summary>
public sealed class FastPostGenerator
{
    private static readonly HashSet<string> SkipSections = new(StringComparer.OrdinalIgnoreCase)
    {
        "see also",
        "browser compatibility",
        "specifications",
        "смотрите также",
        "смотри(те) также",
        "смотри также",
        "см. также",
        "совместимость с браузерами",
        "спецификации",
        "関連情報",
        "ブラウザーの互換性",
        "仕様書",
        "같이 보기",
        "브라우저 호환성",
        "명세서",
        "参见",
        "浏览器兼容性",
        "规范",
        "參見",
        "瀏覽器相容性",
        "規範",
        "voir aussi",
        "compatibilité des navigateurs",
        "spécifications",
        "siehe auch",
        "browser-kompatibilität",
        "spezifikationen",
        "véase también",
        "compatibilidad con navegadores",
        "especificaciones",
        "veja também",
        "compatibilidade com navegadores",
        "especificações",
    };

    private static readonly string[] SkipContains =
    [
        "browser compat",
        "совместимость",
        "ブラウザー互換",
        "브라우저 호환",
        "浏览器兼容",
        "瀏覽器相容",
    ];

    private static readonly string[] ExampleKeywords =
    [
        "example",
        "пример",
        "примеры",
        "例",
        "示例",
        "사용 예",
        "exemple",
        "beispiel",
        "ejemplo",
        "exemplo",
    ];

    public IReadOnlyList<OriginalPost> Generate(IReadOnlyList<MdnSection> sections)
    {
        ArgumentNullException.ThrowIfNull(sections);

        var posts = new List<OriginalPost>();
        OriginalPostBuilder? current = null;
        var pos = 0;

        for (var i = 0; i < sections.Count; i++)
        {
            var section = sections[i];

            if (section.IsH3)
            {
                if (current is not null)
                    AppendH3(current, section);
                continue;
            }

            FlushCurrent(posts, current, ref pos);
            current = null;

            if (ShouldSkip(section.SectionTitle))
                continue;

            current = StartNewPost(section, i, pos);
        }

        FlushCurrent(posts, current, ref pos);
        return posts;
    }

    private static OriginalPostBuilder StartNewPost(MdnSection section, int index, int position)
    {
        var kind = ClassifySection(section.SectionTitle, position);
        var key = SourceKeyFor(section, index);
        var body = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(section.Content))
        {
            body.Append(section.Content.Trim());
        }

        return new OriginalPostBuilder(key, kind, section.SectionTitle, body);
    }

    private static void AppendH3(OriginalPostBuilder builder, MdnSection section)
    {
        if (builder.Body.Length > 0)
        {
            builder.Body.AppendLine();
            builder.Body.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(section.SectionTitle))
        {
            builder.Body.Append(CultureInfo.InvariantCulture, $"### {section.SectionTitle}");
            builder.Body.AppendLine();
            builder.Body.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(section.Content))
        {
            builder.Body.Append(section.Content.Trim());
        }
    }

    private static void FlushCurrent(List<OriginalPost> posts, OriginalPostBuilder? builder, ref int position)
    {
        if (builder is null)
            return;

        var body = builder.Body.ToString().Trim();
        if (string.IsNullOrWhiteSpace(body))
            return;

        posts.Add(new OriginalPost(builder.Key, builder.Kind, builder.Title, body, position));
        position++;
    }

    private static bool ShouldSkip(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        if (SkipSections.Contains(title))
            return true;

        var lower = title.ToLowerInvariant();
        return SkipContains.Any(k => lower.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static PostKind ClassifySection(string? title, int position)
    {
        _ = position;

        if (string.IsNullOrWhiteSpace(title))
            return PostKind.Summary;

        var lower = title.ToLowerInvariant();

        if (ExampleKeywords.Any(k => lower.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return PostKind.Example;

        return PostKind.Concept;
    }

    private static string SourceKeyFor(MdnSection section, int index)
        => !string.IsNullOrWhiteSpace(section.Id)
            ? section.Id.Trim().ToLowerInvariant()
            : $"section-{index.ToString(CultureInfo.InvariantCulture)}";

    private sealed record OriginalPostBuilder(
        string Key,
        PostKind Kind,
        string? Title,
        StringBuilder Body);
}
