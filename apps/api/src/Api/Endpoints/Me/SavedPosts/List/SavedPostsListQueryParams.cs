using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Me.SavedPosts.List;

public sealed class SavedPostsListQueryParams
{
  [FromQuery(Name = "cursor")]
  [StringLength(512)]
  public string? Cursor { get; init; }

  [FromQuery(Name = "limit")]
  [Range(1, 50)]
  [DefaultValue(20)]
  public int? Limit { get; init; }
}
