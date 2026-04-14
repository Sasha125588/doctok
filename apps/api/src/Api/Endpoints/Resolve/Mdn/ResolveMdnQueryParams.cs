using System.ComponentModel.DataAnnotations;
using Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Resolve.Mdn;

public sealed class ResolveMdnQueryParams : LangQueryParams
{
  [FromQuery(Name = "externalRef")]
  [StringLength(512)]
  [Required]
  public string? ExternalRef { get; init; }
}
