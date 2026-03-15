using System.ComponentModel.DataAnnotations;
using Api.Features.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Topics._slug_.Links;

public sealed class TopicsGetLinksQueryParams : LangQueryParams
{
  [FromQuery(Name = "slug")]
  [Required]
  [StringLength(512)]
  public string? Slug { get; init; }
}
