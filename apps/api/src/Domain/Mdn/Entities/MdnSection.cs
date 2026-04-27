namespace Domain.Mdn;

public sealed record MdnSection(
  string? Id,
  string? SectionTitle,
  bool IsH3,
  string Content);
