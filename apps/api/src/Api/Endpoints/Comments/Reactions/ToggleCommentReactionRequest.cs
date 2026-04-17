using System.ComponentModel.DataAnnotations;
using Domain.Reactions;

namespace Api.Endpoints.Comments.Reactions;

public sealed record ToggleCommentReactionRequest(
  [property: EnumDataType(typeof(ReactionValue))] ReactionValue Value
);
