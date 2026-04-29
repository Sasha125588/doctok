using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Posts.Content;

public sealed class PostsGetContentQueryParams
{
  [FromQuery(Name = "variant")]
  [StringLength(64)]
  [DefaultValue("original")]
  public string? Variant { get; init; }
}
