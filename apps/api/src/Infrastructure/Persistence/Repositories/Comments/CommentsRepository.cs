using Dapper;
using Domain.Comments;
using Domain.Shared;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class CommentsRepository(IDbConnectionFactory dbf)
{
  public async Task<CommentView> CreateRoot(long postId, Guid userId, string body, CancellationToken ct)
  {
    await using var conn = dbf.Create();
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
                               returning id, post_id, user_id, parent_comment_id, body,
                                         created_at, updated_at, deleted_at,
                                         like_count, dislike_count
                             )
                             select id, post_id, user_id, parent_comment_id, body,
                                    created_at, updated_at, deleted_at,
                                    like_count, dislike_count,
                                    0 as reply_count
                             from inserted
                             """;

    var row = await conn.QuerySingleOrDefaultAsync<CommentView>(
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

  public async Task<CommentView> Reply(long parentCommentId, Guid userId, string body, CancellationToken ct)
  {
    await using var conn = dbf.Create();
    await conn.OpenAsync(ct);
    await using var tx = await conn.BeginTransactionAsync(ct);

    const string getParentSql = """
                                select id, post_id
                                from comments
                                where id = @parentCommentId
                                   and deleted_at is null
                                for update
                                """;

    var parent = await conn.QuerySingleOrDefaultAsync<(long id, long post_id)>(
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
                             returning id, post_id, user_id, parent_comment_id, body,
                                       created_at, updated_at, deleted_at,
                                       like_count, dislike_count,
                                       0 as reply_count
                             """;

    var row = await conn.QuerySingleAsync<CommentView>(
      new CommandDefinition(
        insertSql,
        new { postId, userId, parentCommentId, body },
        transaction: tx,
        cancellationToken: ct));

    await tx.CommitAsync(ct);
    return row;
  }

  public async Task<IReadOnlyList<CommentView>> ListRoots(
    long postId,
    CommentsCursor? cursor,
    int limit,
    CancellationToken ct)
  {
    const string sql = """
                       select
                         c.id,
                         c.post_id,
                         c.user_id,
                         c.parent_comment_id,
                         c.body,
                         c.created_at,
                         c.updated_at,
                         c.deleted_at,
                         c.like_count,
                         c.dislike_count,
                         (
                           select count(*)::int
                           from comments r
                           where r.parent_comment_id = c.id
                             and r.deleted_at is null
                         ) as reply_count
                       from comments c
                       where c.post_id = @postId
                         and c.parent_comment_id is null
                         and (
                           @cursorId is null
                             or (c.created_at, c.id) < (@cursorCreatedAt, @cursorId)
                         )
                       order by c.created_at desc, c.id desc
                       limit @limit
                       """;

    using var db = dbf.Create();

    var rows = await db.QueryAsync<CommentView>(
      new CommandDefinition(
        sql,
        new
        {
          postId,
          cursorId = cursor?.Id,
          cursorCreatedAt = cursor?.CreatedAt,
          limit,
        },
        cancellationToken: ct));

    return rows.ToList();
  }

  public async Task<IReadOnlyList<CommentView>> ListReplies(
    long commentId,
    CommentsCursor? cursor,
    int limit,
    CancellationToken ct)
  {
    const string sql = """
                       select
                         c.id,
                         c.post_id,
                         c.user_id,
                         c.parent_comment_id,
                         c.body,
                         c.created_at,
                         c.updated_at,
                         c.deleted_at,
                         c.like_count,
                         c.dislike_count,
                         (
                           select count(*)::int
                           from comments r
                           where r.parent_comment_id = c.id
                             and r.deleted_at is null
                         ) as reply_count
                       from comments c
                       where c.parent_comment_id = @commentId
                         and (
                           @cursorId is null
                             or (c.created_at, c.id) > (@cursorCreatedAt, @cursorId)
                         )
                       order by c.created_at asc, c.id asc
                       limit @limit
                       """;

    using var db = dbf.Create();
    var rows = await db.QueryAsync<CommentView>(
      new CommandDefinition(
        sql,
        new
        {
          commentId,
          cursorId = cursor?.Id,
          cursorCreatedAt = cursor?.CreatedAt,
          limit,
        },
        cancellationToken: ct));

    return rows.ToList();
  }

  public async Task<bool> Delete(long commentId, Guid userId, CancellationToken ct)
  {
    await using var conn = dbf.Create();
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
