using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Replies.Create;

public sealed record Request(string Body);

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPost("/comments/{commentId:long}/replies", async (
        long commentId,
        Request req,
        ClaimsPrincipal user,
        CommentsRepository repo,
        CancellationToken ct) =>
      {
        var userId = CurrentUser.GetUserIdOrThrow(user);

        if (string.IsNullOrWhiteSpace(req.Body))
        {
          throw new ArgumentException("Comment body cannot be empty.");
        }

        if (req.Body.Length > 2000)
        {
          throw new ArgumentException("Comment body exceeds 2000 characters.");
        }

        var dto = await repo.Reply(commentId, userId, req.Body.Trim(), ct);
        return Results.Ok(dto);
      })
      .RequireAuthorization()
      .WithTags("Comments");
  }
}
