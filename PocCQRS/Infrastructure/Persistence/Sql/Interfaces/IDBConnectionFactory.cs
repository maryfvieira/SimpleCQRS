using System.Data;

namespace PocCQRS.Infrastructure.Persistence.Sql.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}