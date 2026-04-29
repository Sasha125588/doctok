using Dapper;
using Domain.Reactions;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class BaseReactionsRepository(IDbConnectionFactory dbf)
{
    internal async Task<ReactionView?> Toggle(
        ReactionToggleSql sql,
        long targetId,
        Guid userId,
        ReactionValue value,
        CancellationToken ct)
    {
        await using var conn = dbf.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var target = await conn.QuerySingleOrDefaultAsync<ReactionCounts>(
            new CommandDefinition(
                sql.LockTarget,
                new { targetId },
                transaction: tx,
                cancellationToken: ct));

        if (target is null)
        {
          await tx.RollbackAsync(ct);
          return null;
        }

        var dbValue = value.ToString().ToLowerInvariant();

        var currentValue = await conn.ExecuteScalarAsync<string?>(
            new CommandDefinition(
                sql.SelectCurrentReaction,
                new { targetId, userId },
                transaction: tx,
                cancellationToken: ct));

        var operation = currentValue switch
        {
          null => ReactionWriteOperation.Insert,
          _ when currentValue == dbValue => ReactionWriteOperation.Delete,
          _ => ReactionWriteOperation.Update,
        };

        ReactionValue? resultingValue = operation == ReactionWriteOperation.Delete ? null : value;
        var likeDelta =
            (currentValue == "like" ? -1 : 0)
            + (resultingValue == ReactionValue.Like ? 1 : 0);
        var dislikeDelta =
            (currentValue == "dislike" ? -1 : 0)
            + (resultingValue == ReactionValue.Dislike ? 1 : 0);

        var writeSql = operation switch
        {
          ReactionWriteOperation.Insert => sql.InsertReaction,
          ReactionWriteOperation.Update => sql.UpdateReaction,
          ReactionWriteOperation.Delete => sql.DeleteReaction,
          _ => throw new InvalidOperationException($"Unknown reaction operation: {operation}."),
        };

        await conn.ExecuteAsync(
            new CommandDefinition(
                writeSql,
                new { targetId, userId, value = dbValue },
                transaction: tx,
                cancellationToken: ct));

        var updatedCounts = await conn.QuerySingleAsync<ReactionCounts>(
            new CommandDefinition(
                sql.UpdateCounters,
                new { targetId, likeDelta, dislikeDelta },
                transaction: tx,
                cancellationToken: ct));

        await tx.CommitAsync(ct);

        return new ReactionView(
            resultingValue ?? ReactionValue.None,
            updatedCounts.LikeCount,
            updatedCounts.DislikeCount);
    }

    private sealed record ReactionCounts(int LikeCount, int DislikeCount);

    private enum ReactionWriteOperation
    {
      Insert,
      Update,
      Delete,
    }

    internal sealed record ReactionToggleSql(
      string LockTarget,
      string SelectCurrentReaction,
      string InsertReaction,
      string UpdateReaction,
      string DeleteReaction,
      string UpdateCounters);
}
