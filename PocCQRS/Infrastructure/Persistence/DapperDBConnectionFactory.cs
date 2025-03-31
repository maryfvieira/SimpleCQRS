using System.Data;
using MySql.Data.MySqlClient;

namespace PocCQRS.Infrastructure.Persistence;

public class DapperDbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public DapperDbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_configuration.GetConnectionString("MySQL"));
    }
}