using Domain.Reactions;

namespace Api.Endpoints.Posts.Reactions;

public sealed record Command(long PostId, Guid UserId, ReactionValue Value);
