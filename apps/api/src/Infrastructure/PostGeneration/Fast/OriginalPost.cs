using Domain.Posts;

namespace Infrastructure.PostGeneration.Fast;

public sealed record OriginalPost(
    string SourceSectionKey,
    PostKind Kind,
    string? Title,
    string Body,
    int Position);
