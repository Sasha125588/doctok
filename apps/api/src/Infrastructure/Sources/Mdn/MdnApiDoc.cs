namespace Infrastructure.Sources.Mdn;

public sealed record MdnApiSection(string? Id, string? SectionTitle, string Content);

public sealed record MdnApiDoc(
    string Title,
    string Slug,
    IReadOnlyList<MdnApiSection> Sections,
    string? PageType,
    double? Popularity,
    DateTimeOffset? SourceModifiedAt,
    IReadOnlyList<string> OtherLocales);
