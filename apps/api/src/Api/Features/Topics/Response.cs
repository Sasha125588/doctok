namespace Api.Features.Topics;

public sealed record Response(long Id, string Kind, string Body, int Position, int LikeCount, int DislikeCount, int CommentCount);
