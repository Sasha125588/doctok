using Api.Extensions;
using Domain.Posts;
using ErrorOr;
using Infrastructure.Persistence.Repositories;

namespace Api.Endpoints.Posts.Content;

public sealed class Handler(PostsRepository postsRepo) : IHandler
{
  public async Task<ErrorOr<PostContentView>> Handle(Query query, CancellationToken ct)
  {
    var variant = NormalizeVariant(query.Variant);
    var content = await postsRepo.GetContent(query.PostId, variant, ct);

    return content is null
      ? Error.NotFound(
        code: "Posts.Content.NotFound",
        description: $"Post '{query.PostId}' was not found.")
      : content;
  }

  private static string NormalizeVariant(string? variant)
    => string.IsNullOrWhiteSpace(variant)
      ? "original"
      : variant.Trim().ToLowerInvariant();
}
