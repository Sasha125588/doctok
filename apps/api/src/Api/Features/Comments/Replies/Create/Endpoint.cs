using System.Security.Claims;
using Api.Auth;
using Api.Extensions;
using Api.Features.Comments.Shared;
using Infrastructure.Persistence.Repos.Comments;

namespace Api.Features.Comments.Replies.Create;

public sealed record CreateCommentReplyRequest(string Body);

public sealed class Endpoint : IEndpoint
{
  public void Map(IEndpointRouteBuilder app)
  {
    app.MapPost("/comments/{commentId:long}/replies", async (
        long commentId,
        CreateCommentReplyRequest req,
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

        var response = (await commentsRepo.Reply(commentId, userId, req.Body.Trim(), ct)).ToResponse();
        return Results.Created($"/api/comments/{commentId}/replies/{response.Id}", response);
      })
      .RequireAuthorization()
      .WithTags("Comments")
      .WithSummary("Adds a reply to a root comment")
      .WithName("CommentsRepliesCreate")
      .Produces<CommentResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status404NotFound);
  }
}
