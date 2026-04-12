using Domain.Models;

namespace Infrastructure.Persistence.Repositories;

public class CommentReactionsRepository(BaseReactionsRepository baseReactionsRepo)
{
  private const string ToggleSql = """
      -- params: @comment_id bigint, @user_id uuid, @value text ('like'|'dislike')
      
      with lock_key as (
        select pg_advisory_xact_lock(
          hashtextextended(@comment_id::text || ':' || @user_id::text, 0)
        ) as locked
      ),
      old as (
        select v.value::text as old_value
        from comment_reactions v
        where v.comment_id = @comment_id and v.user_id = @user_id
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
        insert into comment_reactions(comment_id, user_id, value)
        select @comment_id, @user_id, d.new_value::t_reaction_value
        from decision d
        where d.op = 'insert'
        returning 1
      ),
      write_update as (
        update comment_reactions v
        set value = d.new_value::t_reaction_value,
            updated_at = now()
        from decision d
        where d.op = 'update'
          and v.comment_id = @comment_id and v.user_id = @user_id
        returning 1
      ),
      write_delete as (
        delete from comment_reactions v
        using decision d
        where d.op = 'delete'
          and v.comment_id = @comment_id and v.user_id = @user_id
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
      update_comment as (
        update comments c
        set likes_count = greatest(c.likes_count + d.like_delta, 0),
            dislikes_count = greatest(c.dislikes_count + d.dislike_delta, 0)
        from delta d
        where c.id = @comment_id
        returning c.likes_count, c.dislikes_count
      )
      select
        u.likes_count,
        u.dislikes_count,
        coalesce(d.resulting_vote, 'none') as my_vote
      from update_comment u
      cross join delta d;
      """;

  public Task<ReactionResult?> Toggle(long commentId, Guid userId, string value, CancellationToken ct)
    => baseReactionsRepo.Toggle(ToggleSql, new
    {
      comment_id = commentId,
      user_id = userId,
      value
    }, ct);
}
