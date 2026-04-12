using System.Data;
using Npgsql;

namespace Infrastructure.Persistence.ConnectionFactory;

public sealed class DbConnectionFactory(NpgsqlDataSource dataSource): IDbConnectionFactory
{
    public IDbConnection Create() => dataSource.CreateConnection();
}
