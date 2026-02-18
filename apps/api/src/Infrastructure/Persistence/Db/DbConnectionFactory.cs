using System.Data;
using Npgsql;

namespace Infrastructure.Persistence.Db;

public sealed class DbConnectionFactory(NpgsqlDataSource dataSource): IDbConnectionFactory
{
    public IDbConnection Create() => dataSource.CreateConnection();
}