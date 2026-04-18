using Domain.Reactions;

namespace Api.Endpoints.Comments.Reactions;

public sealed record ToggleCommentReactionRequest(ReactionValue Value);
