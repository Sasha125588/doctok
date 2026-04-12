using System.ComponentModel.DataAnnotations;
using Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Topics._slug_.Links;

public sealed class TopicsGetLinksQueryParams : LangQueryParams
{
  [FromQuery(Name = "slug")]
  [Required]
  [StringLength(512)]
  public string? Slug { get; init; }
}
