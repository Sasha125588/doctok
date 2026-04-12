using System.Data.Common;
using Dapper;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class CommentsRepository(IDbConnectionFactory dbf)
{
  private sealed record CommentRow(
    long Id,
    long PostId,
    Guid UserId,
    long? ParentCommentId,
    string Body,
    DateTime CreatedAt,
    DateTime? DeletedAt
  );

  public async Task<Domain.Models.Comment> CreateRoot(long postId, Guid userId, string body, CancellationToken ct)
  {
    using var db = dbf.Create();
    await ((DbConnection)db).OpenAsync(ct);
    await using var tx = await ((DbConnection)db).BeginTransactionAsync(ct);

    const string insertSql = """
                             with post as (
                               update posts
                               set comment_count = comment_count + 1
                               where id = @postId
                               returning id
                             ),
                             inserted as (
                               insert into comments(post_id, user_id, parent_comment_id, body)
                               select post.id, @userId, null, @body
                               from post
                               returning id, post_id, user_id, parent_comment_id, body, created_at, deleted_at
                             )
                             select id, post_id, user_id, parent_comment_id, body, created_at, deleted_at
                             from inserted
                             """;

    var row = await db.QuerySingleOrDefaultAsync<CommentRow>(
      new CommandDefinition(
        insertSql,
        new { postId, userId, body },
        transaction: tx,
        cancellationToken: ct));

    if (row is null)
    {
      await tx.RollbackAsync(ct);
      throw new KeyNotFoundException("Post not found");
    }

    await tx.CommitAsync(ct);
    return ToModel(row);
  }

  public async Task<Domain.Models.Comment> Reply(long parentCommentId, Guid userId, string body, CancellationToken ct)
  {
    using var db = dbf.Create();
    await ((DbConnection)db).OpenAsync(ct);
    await using var tx = await ((DbConnection)db).BeginTransactionAsync(ct);

    const string getParentSql = """
                                select id, post_id, parent_comment_id
                                from comments
                                where id = @parentCommentId
                                   and deleted_at is null
                                for update
                                """;

    var parent = await db.QuerySingleOrDefaultAsync<(long id, long post_id, long? parent_comment_id)>(
      new CommandDefinition(getParentSql, new { parentCommentId }, transaction: tx, cancellationToken: ct));

    if (parent == default)
    {
      throw new KeyNotFoundException("Parent comment not found");
    }

    const string bumpSql = """
                           update posts
                           set comment_count = comment_count + 1
                           where id = @postId
                           returning id
                           """;

    var postId = await db.QuerySingleOrDefaultAsync<long?>(
      new CommandDefinition(
        bumpSql,
        new { postId = parent.post_id },
        transaction: tx,
        cancellationToken: ct));

    if (postId is null)
    {
      await tx.RollbackAsync(ct);
      throw new KeyNotFoundException("Parent comment not found");
    }

    const string insertSql = """
                             insert into comments(post_id, user_id, parent_comment_id, body)
                             values(@postId, @userId, @parentCommentId, @body)
                             returning id, post_id, user_id, parent_comment_id, body, created_at, deleted_at
                             """;

    var row = await db.QuerySingleAsync<CommentRow>(
      new CommandDefinition(
        insertSql,
        new { postId, userId, parentCommentId, body },
        transaction: tx,
        cancellationToken: ct));

    await tx.CommitAsync(ct);
    return ToModel(row);
  }

  public async Task<IReadOnlyList<Domain.Models.Comment>> ListRoots(long postId, int limit, CancellationToken ct)
  {
    const string sql = """
                       select id, post_id, user_id, parent_comment_id, body, created_at, deleted_at
                       from comments
                       where post_id = @postId
                            and parent_comment_id is null
                       order by created_at desc
                       limit @limit
                       """;

    using var db = dbf.Create();
    var rows = await db.QueryAsync<CommentRow>(
      new CommandDefinition(
        sql,
        new { postId, limit },
        cancellationToken: ct));

    return rows.Select(ToModel).ToList();
  }

  public async Task<IReadOnlyList<Domain.Models.Comment>> ListReplies(long commentId, int limit, CancellationToken ct)
  {
    const string sql = """
                       select id, post_id, user_id, parent_comment_id, body, created_at, deleted_at
                       from comments
                       where parent_comment_id = @commentId
                       order by created_at asc
                       limit @limit
                       """;

    using var db = dbf.Create();
    var rows = await db.QueryAsync<CommentRow>(
      new CommandDefinition(
        sql,
        new { commentId, limit },
        cancellationToken: ct));

    return rows.Select(ToModel).ToList();
  }

  public async Task<bool> Delete(long commentId, Guid userId, CancellationToken ct)
  {
    using var db = dbf.Create();
    await ((DbConnection)db).OpenAsync(ct);
    await using var tx = await ((DbConnection)db).BeginTransactionAsync(ct);

    const string delSql = """
                          update comments
                          set deleted_at = now()
                          where id = @commentId
                            and user_id = @userId
                            and deleted_at is null
                          returning post_id
                          """;

    var postId = await db.QuerySingleOrDefaultAsync<long?>(
      new CommandDefinition(
        delSql,
        new { commentId, userId },
        transaction: tx,
        cancellationToken: ct));

    if (postId is null)
    {
      await tx.CommitAsync(ct);
      return false;
    }

    const string decSql = """
                          update posts
                          set comment_count = comment_count -1
                          where id = @postId
                          """;

    await db.ExecuteAsync(
      new CommandDefinition(
        decSql,
        new { postId },
        transaction: tx,
        cancellationToken: ct));

    await tx.CommitAsync(ct);
    return true;
  }

  private static Domain.Models.Comment ToModel(CommentRow r) => new (
    Id: r.Id,
    PostId: r.PostId,
    UserId: r.UserId,
    ParentCommentId: r.ParentCommentId,
    Body: r.Body,
    CreatedAt: new DateTimeOffset(r.CreatedAt),
    IsDeleted: r.DeletedAt is not null
  );
}
