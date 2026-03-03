using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Shared;

public sealed record CommentResponse(
  long Id,
  long PostId,
  Guid UserId,
  long? ParentCommentId,
  string Body,
  DateTimeOffset CreatedAt,
  bool IsDeleted
);

public static class CommentResponseMapper
{
  public static CommentResponse ToResponse(this CommentDto dto) => new (
    dto.Id,
    dto.PostId,
    dto.UserId,
    dto.ParentCommentId,
    dto.Body,
    dto.CreatedAt,
    dto.IsDeleted);

  public static IReadOnlyList<CommentResponse> ToResponses(this IReadOnlyList<CommentDto> items) =>
    items.Select(ToResponse).ToList();
}
