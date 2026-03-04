using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Api.Features.Posts.Comments.Create;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Replies.Create;

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPost("/comments/{commentId:long}/replies", async (
        long commentId,
        CreateCommentRequest req,
        ClaimsPrincipal user,
        CommentsRepository commentsRepo,
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

        var comment = await commentsRepo.Reply(commentId, userId, req.Body.Trim(), ct);
        return Results.Created($"/api/comments/{commentId}/replies/{comment.Id}", comment);
      })
      .RequireAuthorization()
      .WithTags("Comments")
      .WithSummary("Adds a reply to a root comment")
      .WithName("CommentsRepliesCreate")
      .Produces<Domain.Models.Comment>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status404NotFound);
  }
}
