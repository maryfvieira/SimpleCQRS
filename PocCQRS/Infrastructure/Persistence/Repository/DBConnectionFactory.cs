using System.Data;
using MySql.Data.MySqlClient;

namespace PocCQRS.Infrastructure.Persistence.Repository;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
}