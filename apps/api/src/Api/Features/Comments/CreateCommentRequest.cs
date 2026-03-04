using System.ComponentModel.DataAnnotations;

namespace Api.Features.Comments;

public sealed record CreateCommentRequest(
    [property: Required, StringLength(2000, MinimumLength = 1)] string Body
);
