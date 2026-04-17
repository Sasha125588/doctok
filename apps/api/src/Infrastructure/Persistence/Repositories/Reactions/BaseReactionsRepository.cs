using System.Data.Common;
using Dapper;
using Domain.Reactions;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class BaseReactionsRepository(IDbConnectionFactory dbf)
{
    public async Task<ReactionView?> Toggle(string sql, object param, CancellationToken ct)
    {
        await using var conn = (DbConnection)dbf.Create();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var row = await conn.QuerySingleOrDefaultAsync<ReactionView>(
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

        return row;
    }
}
