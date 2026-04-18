using Domain.Reactions;

namespace Infrastructure.Persistence.Repositories;

public class PostReactionsRepository(BaseReactionsRepository baseReactionsRepo)
{
      /* language=postgresql */
      private const string ToggleSql = """
    -- params: @post_id bigint, @user_id uuid, @value text ('like'|'dislike')

    with lock_key as (
      select pg_advisory_xact_lock(
        hashtextextended(@post_id::text || ':' || @user_id::text, 0)
      ) as locked
    ),
    old as (
      select v.value::t_reaction_value as old_value
      from post_reactions v
      where v.post_id = @post_id and v.user_id = @user_id
    ),
    decision as (
      select
        (select old_value from old) as old_value,
        @value::t_reaction_value               as new_value,
        case
          when (select old_value from old) is null then 'insert'
          when (select old_value from old) = @value::t_reaction_value then 'delete'
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
      d.resulting_vote as my_vote,
      u.like_count,
      u.dislike_count
    from update_post u
    cross join delta d;
    """;

      public Task<ReactionView?> Toggle(long postId, Guid userId, string value, CancellationToken ct)
        => baseReactionsRepo.Toggle(ToggleSql, new
        {
          post_id = postId,
          user_id = userId,
          value
        }, ct);
}
