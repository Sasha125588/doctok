using Domain.Common;

namespace Api.Features.Posts.Votes.Toggle;

public sealed record TogglePostVoteRequest(VoteValue Value);
