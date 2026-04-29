namespace Domain.Mdn;

public sealed record MdnSection(
  string? Id,
  string? Title,
  bool IsH3,
  string Content);
