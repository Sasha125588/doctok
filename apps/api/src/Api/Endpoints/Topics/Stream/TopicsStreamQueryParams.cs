using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Topics.Stream;

public sealed class TopicsStreamQueryParams : LangQueryParams
{
  [FromQuery(Name = "slug")]
  [StringLength(512)]
  [DefaultValue("mdn/web/api/element/scrollheight")]
  [Required]
  public string? Slug { get; init; }
}
