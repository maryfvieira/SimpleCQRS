using System.Net.Sockets;
using global::PocCQRS.Application.EventHandlers;
using global::PocCQRS.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;

namespace PocCQRS.EntryPoint.Consumer;

public class EventConsumer2<T> : IConsumer<T> where T : class, IDomainEvent
{
    private readonly ILogger<EventConsumer<T>> _logger;
    private readonly IEventHandler<T> _handler;

    public EventConsumer2(
        ILogger<EventConsumer<T>> logger,
        IEventHandler<T> handler)
    {
        _logger = logger;
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        var retryPolicy = Policy
            .Handle<SocketException>() // ou outra exception específica da sua regra de negócio
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, ctx) =>
                {
                    _logger.LogWarning(exception, "Retry {RetryCount} for event {EventType}", retryCount, typeof(T).Name);
                });

        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                _logger.LogInformation("Consuming event: {EventType}", typeof(T).Name);
                await _handler.HandleAsync(context.Message);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento {EventType}. Enviando para DLQ após falhas.", typeof(T).Name);
            throw; // relança para que MassTransit trate como falha
        }
    }
}