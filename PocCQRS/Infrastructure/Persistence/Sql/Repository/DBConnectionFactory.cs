using System.Data;
using MySql.Data.MySqlClient;
using PocCQRS.Infrastructure.Persistence.Sql.Interfaces;

namespace PocCQRS.Infrastructure.Persistence.Sql.Repository;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
}