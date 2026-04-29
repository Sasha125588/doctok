using Domain.Comments;

namespace Api.Endpoints.Comments;

public sealed record CommentsResponse(IReadOnlyList<CommentView> Items, string? NextCursor);
