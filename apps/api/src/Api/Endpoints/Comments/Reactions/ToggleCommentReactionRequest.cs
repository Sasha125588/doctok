using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Api.Endpoints.Comments.Reactions;

public sealed record ToggleCommentReactionRequest(
  [property: EnumDataType(typeof(ReactionValue))] ReactionValue Value
);
