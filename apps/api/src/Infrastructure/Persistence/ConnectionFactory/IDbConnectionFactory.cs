using System.Data;

namespace Infrastructure.Persistence.ConnectionFactory;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}
