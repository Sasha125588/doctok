using System.Data.Common;
using Dapper;
using Domain.Models;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class BaseReactionsRepository(IDbConnectionFactory dbf)
{
    public async Task<ReactionResult?> Toggle(string sql, object param, CancellationToken ct)
    {
        await using var conn = (DbConnection)dbf.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var row = await conn.QuerySingleOrDefaultAsync<DbRow>(
            new CommandDefinition(
                sql,
                param,
                transaction: tx,
                cancellationToken: ct));

        if (row is null)
        {
          await tx.RollbackAsync(ct);
          return null;
        }

        await tx.CommitAsync(ct);

        return new ReactionResult(
            MyVote: row.MyVote,
            LikeCount: row.LikeCount,
            DislikeCount: row.DislikeCount);
    }

    private sealed record DbRow(int LikeCount, int DislikeCount, string MyVote);
}
