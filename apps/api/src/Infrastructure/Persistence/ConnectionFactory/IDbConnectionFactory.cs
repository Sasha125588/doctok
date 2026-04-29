using System.Data.Common;

namespace Infrastructure.Persistence.ConnectionFactory;

public interface IDbConnectionFactory
{
    DbConnection Create();
}
