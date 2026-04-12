using Domain.Common;

namespace Api.Endpoints.Comments.Reactions;

public sealed record Command(long CommentId, Guid UserId, ReactionValue Value);
