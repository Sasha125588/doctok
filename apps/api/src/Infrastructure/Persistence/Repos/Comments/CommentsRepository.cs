using System.Data.Common;
using Dapper;

using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Comments;
public sealed class CommentsRepository(IDbConnectionFactory dbf)
{
  private sealed record CommentRow(
    long id,
    long post_id,
    Guid user_id,
    long? parent_comment_id,
    string body,
    DateTime created_at,
    DateTime? deleted_at
  );

  public async Task<CommentDto> CreateRoot(long postId, Guid userId, string body, CancellationToken ct)
  {
    using var db = dbf.Create();
    await ((DbConnection)db).OpenAsync(ct);
    await using var tx = await ((DbConnection)db).BeginTransactionAsync(ct);

    const string insertSql = """
                             insert into comments(post_id, user_id, parent_comment_id, body)
                             values(@postId, @userId, null, @body)
                             returning id, post_id, user_id, parent_comment_id, body, created_at, deleted_at
                             """;

    var row = await db.QuerySingleAsync<CommentRow>(new CommandDefinition(insertSql, new { postId, userId, body }, transaction: tx, cancellationToken: ct));

    const string bumpSql = """
                           update posts
                           set comment_count = comment_count + 1
                           where id = @postId
                           """;

    var affected = await db.ExecuteAsync(new CommandDefinition(bumpSql, new { postId }, transaction: tx, cancellationToken: ct));

    if (affected == 0)
    {
      throw new KeyNotFoundException("Post not found");
    }

    await tx.CommitAsync(ct);
    return ToDto(row);
  }

  public async Task<CommentDto> Reply(long parentCommentId, Guid userId, string body, CancellationToken ct)
  {
    using var db = dbf.Create();
    await ((DbConnection)db).OpenAsync(ct);
    await using var tx = await ((DbConnection)db).BeginTransactionAsync(ct);

    const string getParentSql = """
                                 select id, post_id, parent_comment_id
                                 from comments
                                 where id = @parentCommentId
                                    and deleted_at is null
                                 """;

    var parent = await db.QuerySingleOrDefaultAsync<(long id, long post_id, long? parent_comment_id)>(
      new CommandDefinition(getParentSql, new { parentCommentId }, transaction: tx, cancellationToken: ct));

    if (parent == default) throw new KeyNotFoundException("Parent comment not found");
    if (parent.parent_comment_id is not null) throw new ArgumentException("Replies to replies are not supported");

    var postId = parent.post_id;

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

    const string bumpSql = """
                           update posts
                           set comment_count = comment_count + 1
                           where id = @postId
                           """;

    await db.ExecuteAsync(new CommandDefinition(bumpSql, new { postId }, transaction: tx,  cancellationToken: ct));

    await tx.CommitAsync(ct);
    return ToDto(row);
  }

  public async Task<IReadOnlyList<CommentDto>> ListRoots(long postId, int limit, CancellationToken ct)
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
    var rows = await db.QueryAsync<CommentRow>(new CommandDefinition(sql, new { postId, limit }, cancellationToken: ct));

    return rows.Select(ToDto).ToList();
  }

  public async Task<IReadOnlyList<CommentDto>> ListReplies(long commentId, int limit, CancellationToken ct)
  {
    const string sql = """
                       select id, post_id, user_id, parent_comment_id, body, created_at, deleted_at
                       from comments
                       where parent_comment_id = @commentId
                       order by created_at asc
                       limit @limit
                       """;

    using var db = dbf.Create();
    var rows = await db.QueryAsync<CommentRow>(new CommandDefinition(sql, new { commentId, limit }, cancellationToken: ct));

    return rows.Select(ToDto).ToList();
  }

  public async Task<bool> Delete(long commentId, Guid userId, CancellationToken ct)
  {
    using var db = dbf.Create();
    await ((DbConnection)db).OpenAsync(ct);
    await using var tx = await ((DbConnection)db).BeginTransactionAsync(ct);

    const string delSql = """
                          update comments
                          set deleted_at = now(), body = ''
                          where id = @commentId
                            and user_id = @userId
                            and deleted_at is null
                          returning post_id
                          """;

    var postId = await db.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(delSql, new { commentId, userId }, transaction: tx, cancellationToken: ct));

    // якщо коментарій вже видалений, то виходимо з функції
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

    await db.ExecuteAsync(new CommandDefinition(decSql, new { postId }, transaction: tx, cancellationToken: ct));

    await tx.CommitAsync(ct);
    return true;
  }

  private static CommentDto ToDto(CommentRow r) => new (
    Id: r.id,
    PostId: r.post_id,
    UserId: r.user_id,
    ParentCommentId: r.parent_comment_id,
    Body: r.deleted_at is null ? r.body : "",
    CreatedAt: new DateTimeOffset(r.created_at),
    IsDeleted: r.deleted_at is not null
  );
}

public sealed record CommentDto(
  long Id,
  long PostId,
  Guid UserId,
  long? ParentCommentId,
  string Body,
  DateTimeOffset CreatedAt,
  bool IsDeleted
);
