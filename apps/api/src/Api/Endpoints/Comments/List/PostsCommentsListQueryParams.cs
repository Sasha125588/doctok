using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Comments.List;

public sealed class PostsCommentsListQueryParams
{
  [FromQuery(Name = "limit")]
  [Range(1, 50)]
  [DefaultValue(20)]
  public int? Limit { get; init; }
}
