using System.Net.Sockets;
using MassTransit;
using PocCQRS.Infrastructure.Settings;
using Polly;

namespace PocCQRS.Infrastructure.Messaging;

public class Consumer<T> : IConsumer<T> where T : class
{
    private readonly ILogger<Consumer<T>> _logger;
    private readonly AsyncPolicy _resiliencePolicy;
    private readonly PocCQRS.Infrastructure.Settings.RabbitMQ.QueueSettings _queueConfig;
    private readonly IPublishEndpoint _publishEndpoint;

    public Consumer(ILogger<Consumer<T>> logger, IAppSettings appSettings, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;

        if (!appSettings.RabbitMQSettings.Main.Queues.ContainsKey(typeof(T).Name))
            throw new Exception($"RabbitMQ queue for {typeof(T).Name} does not exist");

        _queueConfig = appSettings.RabbitMQSettings.Main.Queues[typeof(T).Name];

        _resiliencePolicy = CreateResiliencePolicy();
    }

    private AsyncPolicy CreateResiliencePolicy()
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                _queueConfig.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(_queueConfig.RetryInterval, retryAttempt)),
                onRetry: (exception, delay, retryCount, context) =>
                {
                    _logger.LogWarning($"Retry {retryCount} after {delay.TotalSeconds}s due to: {exception.Message}");
                });

        var fallbackPolicy = Policy
            .Handle<Exception>()
            .FallbackAsync(
                fallbackAction: async (context, cancellationToken) =>
                {
                    if (context.TryGetValue("message", out var messageObj) && messageObj is T message)
                        await SendToDlqAsync(message);
                },
                onFallbackAsync: (exception, context) =>
                {
                    _logger.LogError("All retries failed. Sending to DLQ. Error: {ErrorMessage}", exception.Message);
                    return Task.CompletedTask;
                });

        var circuitBreakerPolicy = Policy
            .Handle<RabbitMqConnectionException>()
            .Or<SocketException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: _queueConfig.CircuitBreaker.TripThreshold,
                durationOfBreak: _queueConfig.CircuitBreaker.DurationOfBreak,
                onBreak: (ex, breakDelay) =>
                {
                    _logger.LogWarning("Circuit broken! Will not attempt for {BreakSeconds}s. Error: {ErrorMessage}",
                        breakDelay.TotalSeconds, ex.Message);
                },
                onReset: () => _logger.LogInformation("Circuit reset!"),
                onHalfOpen: () => _logger.LogInformation("Circuit half-open: Testing..."));

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, fallbackPolicy);
    }

    private async Task SendToDlqAsync(T message)
    {
        try
        {
            _logger.LogWarning("Sending message to DLQ: {DLQQueue}", _queueConfig.DLQ.Queue);
            await _publishEndpoint.Publish(message, context =>
            {
                context.Headers.Set("x-reason", "Max retry attempts reached");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to DLQ");
        }
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        await _resiliencePolicy.ExecuteAsync(async () =>
        {
            try
            {
                _logger.LogInformation("Processing message: {Message}", context.Message);
                // Simulação de erro para testar o DLQ
                // throw new InvalidOperationException("Simulated error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message processing failed");
                throw; // Deixa a política de resiliência lidar com o erro
            }
        });
    }
}