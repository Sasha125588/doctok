using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Api.Features.Admin.Mdn.Preload;

public sealed class PreloadMdnRequest
{
  [StringLength(10)]
  [DefaultValue("en")]
  public string? Lang { get; init; }

  [Range(1, 100)]
  [DefaultValue(5)]
  public int? Count { get; init; }

  public int? Seed { get; init; }

  public string? Prefix { get; init; }
}
