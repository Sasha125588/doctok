using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Api.Endpoints.Posts.Reactions;

public sealed record TogglePostReactionRequest(
  [property: EnumDataType(typeof(ReactionValue))] ReactionValue Value
);
