using System.ComponentModel.DataAnnotations;
using Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Resolve.Mdn;

public sealed class ResolveMdnQueryParams : LangQueryParams
{
  [FromQuery(Name = "externalRef")]
  [Required]
  [StringLength(512)]
  public string? ExternalRef { get; init; }
}
