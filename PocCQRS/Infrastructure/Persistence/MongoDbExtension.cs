using Dapper;
using PocCQRS.Infrastructure.Persistence.Repository;
using PocCQRS.Infrastructure.Settings;

namespace PocCQRS.Infrastructure.Persistence;

public static class MongoDbExtension
{
    public static IServiceCollection AddMongoPersistence(this IServiceCollection services, IAppSettings appSettings)
    {
        var config = appSettings.MongoSettings;
        
        var mongoDatabase = MongoDbConfig.ConfigureMongoDb(config.ConnectionString, config.Database);
        services.AddSingleton(mongoDatabase);
        
        return services;
    }
}