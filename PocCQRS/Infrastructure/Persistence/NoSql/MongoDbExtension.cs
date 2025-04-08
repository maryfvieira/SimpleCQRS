using Dapper;
using PocCQRS.Infrastructure.Persistence.NoSql.Client;
using PocCQRS.Infrastructure.Persistence.NoSql.Interfaces;
using PocCQRS.Infrastructure.Persistence.NoSql.Repository;
using PocCQRS.Infrastructure.Settings;

namespace PocCQRS.Infrastructure.Persistence.NoSql;

public static class MongoDbExtension
{
    public static IServiceCollection AddNoSqlPersistence(this IServiceCollection services, IAppSettings appSettings)
    {
        var config = appSettings.MongoSettings;

        var mongoDatabase = MongoDbConfig.ConfigureMongoDb(config.ConnectionString, config.Database);
        services.AddSingleton(mongoDatabase);

        services.AddScoped<IEventStore, MongoEventStore>();
        services.AddScoped<IEventStoreRepository, EventStoreRepository>();

        return services;
    }
}