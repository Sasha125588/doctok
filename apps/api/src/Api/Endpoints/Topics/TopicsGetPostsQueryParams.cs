using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Topics;

public sealed class TopicsGetPostsQueryParams : LangQueryParams
{
  [FromQuery(Name = "slug")]
  [StringLength(512)]
  [Required]
  public required string Slug { get; init; }

  [FromQuery(Name = "limit")]
  [Range(1, 50)]
  [DefaultValue(20)]
  public int? Limit { get; init; }

  [FromQuery(Name = "cursor")]
  [StringLength(512)]
  public string? Cursor { get; init; }
}
