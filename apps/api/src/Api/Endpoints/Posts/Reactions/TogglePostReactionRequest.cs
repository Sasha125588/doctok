using Domain.Reactions;

namespace Api.Endpoints.Posts.Reactions;

public sealed record TogglePostReactionRequest(ReactionValue Value);
