using Microsoft.Extensions.Options;
using PocCQRS.Infrastructure.Persistence.Cache.Client;
using PocCQRS.Infrastructure.Persistence.Cache.Interfaces;
using PocCQRS.Infrastructure.Settings;
using StackExchange.Redis;

namespace PocCQRS.Infrastructure.Persistence.Cache
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddCachePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Redis>(configuration.GetSection(Redis.SectionName));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<Redis>>().Value;
                var config = ConfigurationOptions.Parse($"{settings.Host}");
                config.ConnectTimeout = settings.ConnectTimeout;
                config.ReconnectRetryPolicy = new ExponentialRetry(deltaBackOffMilliseconds: settings.DeltaBackOffMilliseconds);
                config.AbortOnConnectFail = settings.AbortOnConnectFail;

                return ConnectionMultiplexer.Connect(config);
            });

            services
                .AddTransient<ICacheClient, RedisCacheClient>()
                .AddTransient<IAsyncCacheClient, RedisCacheClient>()
                .AddTransient<IManageCacheClient, RedisCacheClient>();

            return services;
        }
    }
}
