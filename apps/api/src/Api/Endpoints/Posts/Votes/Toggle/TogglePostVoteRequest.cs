using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Api.Endpoints.Posts.Votes.Toggle;

public sealed record TogglePostVoteRequest(
  [property: EnumDataType(typeof(VoteValue))] VoteValue Value
);
