using Dapper;
using PocCQRS.Infrastructure.Persistence.Repository;
using PocCQRS.Infrastructure.Settings;

namespace PocCQRS.Infrastructure.Persistence;

public static class MySqlDbExtension
{

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory>(provider => 
        new DbConnectionFactory(provider.GetRequiredService<IAppSettings>().DatabaseSettings.ConnectionString));
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;

    }

    static async Task InitializeDatabase(IServiceProvider services)
    {
        try
        {
            using var scope = services.CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>().CreateConnection();
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Orders (
                    Id CHAR(36) PRIMARY KEY,
                    ProductId VARCHAR(100) NOT NULL,
                    Quantity INT NOT NULL,
                    CreatedAt DATETIME NOT NULL
                )");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }
}