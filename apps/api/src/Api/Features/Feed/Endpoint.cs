using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Domain.Common;
using Infrastructure.Persistence.Repos.Feed;

namespace Api.Features.Feed;

public sealed record FeedItem(
  long PostId,
  string Title,
  string TopicSlug,
  string TopicTitle,
  string Kind,
  string Body,
  int Position,
  int LikeCount,
  int DislikeCount,
  int CommentCount,
  string MyVote // "like" | "dislike" | "none"
);

public sealed class FeedEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
      app.MapGet("/feed", async (
        string? cursor,
        string? lang,
        int? limit,
        ClaimsPrincipal user,
        FeedRepository feedRepo,
        CancellationToken ct) =>
      {
        var take = Math.Clamp(limit ?? 20, 1, 50);
        var resolvedLang = LanguageHelpers.NormalizeLang(lang ?? "en");
        var feedCursor = CursorCodec.Decode(cursor);

        Guid? userId = null;
        if (user.Identity?.IsAuthenticated == true)
          userId = CurrentUser.GetUserIdOrThrow(user);

        var rows = await feedRepo.GetPage(feedCursor, userId, resolvedLang, take, ct);

        var items = rows.Select(r => new FeedItem(
          r.Id,
          r.Title,
          r.Topic_Slug,
          r.Topic_Title,
          r.Kind,
          r.Body,
          r.Position,
          r.Like_Count,
          r.Dislike_Count,
          r.Comment_Count,
          r.My_Vote)).ToList();

        var nextCursor = rows.Count == take
          ? CursorCodec.Encode(new FeedCursor(rows[^1].Popularity, rows[^1].Id))
          : null;

        return Results.Ok(new Response(items, nextCursor));
      });
    }
}
