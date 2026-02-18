using System.Data;

namespace Infrastructure.Persistence.Db;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}