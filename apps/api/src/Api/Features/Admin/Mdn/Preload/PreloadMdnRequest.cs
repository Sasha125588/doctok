using System.ComponentModel.DataAnnotations;

namespace Api.Features.Admin.Mdn.Preload;

public sealed record PreloadMdnRequest(
  [property: StringLength(10)] string Lang,
  [property: Range(1, 100)] int Count,
  int? Seed,
  string? Prefix
);
