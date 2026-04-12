using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Api.Endpoints.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Feed;

public sealed class FeedListQueryParams : LangQueryParams
{
  [FromQuery(Name = "cursor")]
  [StringLength(512)]
  public string? Cursor { get; init; }

  [FromQuery(Name = "limit")]
  [Range(1, 50)]
  [DefaultValue(20)]
  public int? Limit { get; init; }
}
