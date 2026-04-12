using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Api.Endpoints.Common;

namespace Api.Endpoints.Admin.Mdn.Preload;

public sealed class PreloadMdnRequest: LangQueryParams
{
  [Range(1, 100)]
  [DefaultValue(5)]
  public int? Count { get; init; }

  public int? Seed { get; init; }

  public string? Prefix { get; init; }
}
