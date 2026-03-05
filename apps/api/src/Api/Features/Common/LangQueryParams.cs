using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Common;

public abstract class LangQueryParams
{
  [FromQuery(Name = "lang")]
  [StringLength(10)]
  [DefaultValue("en")]
  public string? Lang { get; init; }
}
