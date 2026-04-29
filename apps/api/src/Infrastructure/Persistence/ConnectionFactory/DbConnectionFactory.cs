using System.Data.Common;
using Npgsql;

namespace Infrastructure.Persistence.ConnectionFactory;

public sealed class DbConnectionFactory(NpgsqlDataSource dataSource): IDbConnectionFactory
{
    public DbConnection Create() => dataSource.CreateConnection();
}
