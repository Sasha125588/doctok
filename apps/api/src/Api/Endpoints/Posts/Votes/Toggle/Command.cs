using Domain.Common;

namespace Api.Endpoints.Posts.Votes.Toggle;

public sealed record Command(long PostId, Guid UserId, VoteValue Value);
