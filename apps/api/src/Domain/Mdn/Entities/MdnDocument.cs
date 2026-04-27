namespace Domain.Mdn;

public sealed record MdnDocument(
    string Title,
    string Slug,
    IReadOnlyList<MdnSection> Sections,
    string? PageType,
    double? Popularity,
    DateTimeOffset? SourceModifiedAt,
    IReadOnlyList<string> OtherLocales);
