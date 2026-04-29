using Domain.Reactions;

namespace Infrastructure.Persistence.Repositories;

public class CommentReactionsRepository(BaseReactionsRepository baseReactionsRepo)
{
  private static readonly BaseReactionsRepository.ReactionToggleSql ToggleSql = new(
    LockTarget: """
                select like_count, dislike_count
                from public.comments
                where id = @targetId
                for update
                """,
    SelectCurrentReaction: """
                           select value::text
                           from public.comment_reactions
                           where comment_id = @targetId
                             and user_id = @userId
                           """,
    InsertReaction: """
                    insert into public.comment_reactions(comment_id, user_id, value)
                    values (@targetId, @userId, @value::public.t_reaction_value)
                    """,
    UpdateReaction: """
                    update public.comment_reactions
                    set value = @value::public.t_reaction_value,
                        updated_at = now()
                    where comment_id = @targetId
                      and user_id = @userId
                    """,
    DeleteReaction: """
                    delete from public.comment_reactions
                    where comment_id = @targetId
                      and user_id = @userId
                    """,
    UpdateCounters: """
                    update public.comments
                    set like_count = greatest(like_count + @likeDelta, 0),
                        dislike_count = greatest(dislike_count + @dislikeDelta, 0)
                    where id = @targetId
                    returning like_count, dislike_count
                    """);

  public Task<ReactionView?> Toggle(long commentId, Guid userId, ReactionValue value, CancellationToken ct)
    => baseReactionsRepo.Toggle(ToggleSql, commentId, userId, value, ct);
}
