using Domain.Common;

namespace Api.Features.Posts.Votes.Toggle;

public sealed record Command(long PostId, Guid UserId, VoteValue Value);
