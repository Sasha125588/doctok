using System.Data.Common;
using Dapper;
using Domain.Comments;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class CommentsRepository(IDbConnectionFactory dbf)
{
  public async Task<Comment> CreateRoot(long postId, Guid userId, string body, CancellationToken ct)
  {
    await using var conn = (DbConnection)dbf.Create();
    await conn.OpenAsync(ct);
    await using var tx = await conn.BeginTransactionAsync(ct);

    /* language=postgresql */
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

    var row = await conn.QuerySingleOrDefaultAsync<Comment>(
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
    return row;
  }

  public async Task<Comment> Reply(long parentCommentId, Guid userId, string body, CancellationToken ct)
  {
    await using var conn = (DbConnection)dbf.Create();
    await conn.OpenAsync(ct);
    await using var tx = await conn.BeginTransactionAsync(ct);

    const string getParentSql = """
                                select id, post_id, parent_comment_id
                                from comments
                                where id = @parentCommentId
                                   and deleted_at is null
                                for update
                                """;

    var parent = await conn.QuerySingleOrDefaultAsync<(long id, long post_id, long? parent_comment_id)>(
      new CommandDefinition(getParentSql, new { parentCommentId }, transaction: tx, cancellationToken: ct));

    if (parent == default)
    {
      await tx.RollbackAsync(ct);
      throw new KeyNotFoundException("Parent comment not found");
    }

    const string bumpSql = """
                           update posts
                           set comment_count = comment_count + 1
                           where id = @postId
                           returning id
                           """;

    var postId = await conn.QuerySingleOrDefaultAsync<long?>(
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

    var row = await conn.QuerySingleAsync<Comment>(
      new CommandDefinition(
        insertSql,
        new { postId, userId, parentCommentId, body },
        transaction: tx,
        cancellationToken: ct));

    await tx.CommitAsync(ct);
    return row;
  }

  public async Task<IReadOnlyList<Comment>> ListRoots(long postId, int limit, CancellationToken ct)
  {
    const string sql = """
                       select id, post_id, user_id, parent_comment_id, body, like_count, dislike_count, created_at, deleted_at
                       from comments
                       where post_id = @postId
                            and parent_comment_id is null
                       order by created_at desc
                       limit @limit
                       """;

    using var db = dbf.Create();

    var rows = await db.QueryAsync<Comment>(
      new CommandDefinition(
        sql,
        new { postId, limit },
        cancellationToken: ct));

    return rows.ToList();
  }

  public async Task<IReadOnlyList<Comment>> ListReplies(long commentId, int limit, CancellationToken ct)
  {
    const string sql = """
                       select id, post_id, user_id, parent_comment_id, body, like_count, dislike_count, created_at, deleted_at
                       from comments
                       where parent_comment_id = @commentId
                       order by created_at asc
                       limit @limit
                       """;

    using var db = dbf.Create();
    var rows = await db.QueryAsync<Comment>(
      new CommandDefinition(
        sql,
        new { commentId, limit },
        cancellationToken: ct));

    return rows.ToList();
  }

  public async Task<bool> Delete(long commentId, Guid userId, CancellationToken ct)
  {
    await using var conn = (DbConnection)dbf.Create();
    await conn.OpenAsync(ct);
    await using var tx = await conn.BeginTransactionAsync(ct);

    const string delSql = """
                          update comments
                          set deleted_at = now()
                          where id = @commentId
                            and user_id = @userId
                            and deleted_at is null
                          returning post_id
                          """;

    var postId = await conn.QuerySingleOrDefaultAsync<long?>(
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

    await conn.ExecuteAsync(
      new CommandDefinition(
        decSql,
        new { postId },
        transaction: tx,
        cancellationToken: ct));

    await tx.CommitAsync(ct);
    return true;
  }
}
