using System.Data.Common;
using Dapper;
using Domain.Models;
using Infrastructure.Persistence.Db;

namespace Infrastructure.Persistence.Repos.Votes;

public sealed class VotesRepository(IDbConnectionFactory dbf)
{
    private const string ToggleSql = """
    -- params: @post_id bigint, @user_id uuid, @value text ('like'|'dislike')

    with lock_key as (
      select pg_advisory_xact_lock(
        hashtextextended(@post_id::text || ':' || @user_id::text, 0)
      ) as locked
    ),
    old as (
      select v.value::text as old_value
      from post_reactions v
      where v.post_id = @post_id and v.user_id = @user_id
    ),
    decision as (
      select
        (select old_value from old) as old_value,
        @value::text               as new_value,
        case
          when (select old_value from old) is null then 'insert'
          when (select old_value from old) = @value::text then 'delete'
          else 'update'
        end as op
    ),
    write_insert as (
      insert into post_reactions(post_id, user_id, value)
      select @post_id, @user_id, d.new_value::t_reaction_value
      from decision d
      where d.op = 'insert'
      returning 1
    ),
    write_update as (
      update post_reactions v
      set value = d.new_value::t_reaction_value,
          updated_at = now()
      from decision d
      where d.op = 'update'
        and v.post_id = @post_id and v.user_id = @user_id
      returning 1
    ),
    write_delete as (
      delete from post_reactions v
      using decision d
      where d.op = 'delete'
        and v.post_id = @post_id and v.user_id = @user_id
      returning 1
    ),
    delta as (
      select
        (case
          when op = 'insert' and new_value = 'like' then 1
          when op = 'delete' and old_value = 'like' then -1
          when op = 'update' then
            (case when old_value = 'like' then -1 else 0 end) +
            (case when new_value = 'like' then 1 else 0 end)
          else 0
        end) as like_delta,
        (case
          when op = 'insert' and new_value = 'dislike' then 1
          when op = 'delete' and old_value = 'dislike' then -1
          when op = 'update' then
            (case when old_value = 'dislike' then -1 else 0 end) +
            (case when new_value = 'dislike' then 1 else 0 end)
          else 0
        end) as dislike_delta,
        case when op = 'delete' then null else new_value end as resulting_vote
      from decision
    ),
    update_post as (
      update posts p
      set like_count    = greatest(p.like_count + d.like_delta, 0),
          dislike_count = greatest(p.dislike_count + d.dislike_delta, 0)
      from delta d
      where p.id = @post_id
      returning p.like_count, p.dislike_count
    )
    select
      u.like_count,
      u.dislike_count,
      coalesce(d.resulting_vote, 'none') as my_vote
    from update_post u
    cross join delta d;
    """;

    private sealed record DbRow(int LikeCount, int DislikeCount, string MyVote);

    public async Task<VoteResult?> Toggle(long postId, Guid userId, string value, CancellationToken ct)
    {
        await using var conn = (DbConnection)dbf.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var row = await conn.QuerySingleOrDefaultAsync<DbRow>(
            new CommandDefinition(
                ToggleSql,
                new { post_id = postId, user_id = userId, value },
                transaction: tx,
                cancellationToken: ct));

        if (row is null)
        {
          await tx.RollbackAsync(ct);
          return null;
        }

        await tx.CommitAsync(ct);

        return new VoteResult(
            MyVote: row.MyVote,
            LikeCount: row.LikeCount,
            DislikeCount: row.DislikeCount);
    }
}
