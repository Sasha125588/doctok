using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repos.Topics;

namespace Api.Features.Topics;

public sealed record TopicPostItem(
  long Id,
  string Title,
  string TopicSlug,
  string TopicTitle,
  string Kind,
  string Body,
  int Position,
  int LikeCount,
  int DislikeCount,
  int CommentCount,
  string MyVote,
  double? Popularity
);

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapGet("/topics/{slug}", async (
      string slug,
      string? lang,
      ClaimsPrincipal user,
      TopicReadRepository topicRepo,
      CancellationToken ct) =>
    {
      Guid? userId = null;
      if (user.Identity?.IsAuthenticated == true)
        userId = CurrentUser.GetUserIdOrThrow(user);

      var resolvedLang = LanguageHelpers.NormalizeLang(lang ?? "en");
      var posts = await topicRepo.GetPosts(slug, resolvedLang, userId, ct);

      if (posts.Count == 0)
      {
        return Results.NotFound();
      }

      var items = posts.Select(post => new TopicPostItem(
        post.Id,
        post.Title,
        post.Topic_Slug,
        post.Topic_Title,
        post.Kind,
        post.Body,
        post.Position,
        post.Like_Count,
        post.Dislike_Count,
        post.Comment_Count,
        post.My_Vote,
        post.Popularity)).ToList();

      return Results.Ok(new TopicPostsResponse(items));
    })
    .WithTags("Topics")
    .WithSummary("Returns posts for a topic")
    .WithName("TopicsGetPosts")
    .Produces<TopicPostsResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
  }
}
