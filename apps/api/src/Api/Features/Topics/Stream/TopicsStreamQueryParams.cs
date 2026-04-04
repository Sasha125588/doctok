using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Api.Features.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Topics.Stream;

public sealed class TopicsStreamQueryParams : LangQueryParams
{
  [FromQuery(Name = "slug")]
  [Required]
  [StringLength(512)]
  [DefaultValue("mdn/web/api/element/scrollheight")]
  public string? Slug { get; init; }
}
