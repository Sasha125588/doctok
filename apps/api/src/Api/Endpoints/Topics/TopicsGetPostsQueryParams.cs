using System.ComponentModel.DataAnnotations;
using Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Topics;

public sealed class TopicsGetPostsQueryParams : LangQueryParams
{
  [FromQuery(Name = "slug")]
  [StringLength(512)]
  [Required]
  public string? Slug { get; init; }
}
