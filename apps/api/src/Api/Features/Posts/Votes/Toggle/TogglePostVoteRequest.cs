using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Api.Features.Posts.Votes.Toggle;

public sealed record TogglePostVoteRequest(
  [property: EnumDataType(typeof(VoteValue))] VoteValue Value
);
