using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Api.Features.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Feed.Topics;

public sealed class FeedTopicsListQueryParams : LangQueryParams
{
  [FromQuery(Name = "cursor")]
  [StringLength(512)]
  public string? Cursor { get; init; }

  [FromQuery(Name = "limit")]
  [Range(1, 50)]
  [DefaultValue(20)]
  public int? Limit { get; init; }
}
