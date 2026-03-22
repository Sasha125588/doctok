using System.ComponentModel.DataAnnotations;
using Api.Features.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Resolve.Mdn;

public sealed class ResolveMdnQueryParams : LangQueryParams
{
  [FromQuery(Name = "externalRef")]
  [Required]
  [StringLength(512)]
  public string? ExternalRef { get; init; }
}
