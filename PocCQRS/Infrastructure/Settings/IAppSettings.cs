namespace PocCQRS.Infrastructure.Settings;

public interface IAppSettings
{
    RabbitMQ RabbitMQSettings { get; }
    MySqlDB DatabaseSettings { get; }
    Reddis CacheSettings { get; }
    Task ReloadAsync();
}