namespace PocCQRS.Infrastructure.Persistence.NoSql.Client;

// OrderSystem.Infrastructure/Persistence/MongoDbConfig.cs
using MongoDB.Driver;

public static class MongoDbConfig
{
    public static IMongoDatabase ConfigureMongoDb(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        return client.GetDatabase(databaseName);
    }
}
