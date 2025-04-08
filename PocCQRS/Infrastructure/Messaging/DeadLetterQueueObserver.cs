using MassTransit;

namespace PocCQRS.Infrastructure.Messaging;


public class DeadLetterQueueObserver : IReceiveObserver
{
    private readonly string _dlqQueue;

    public DeadLetterQueueObserver(string dlqQueue)
    {
        _dlqQueue = dlqQueue;
    }

    public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class => Task.CompletedTask;
    public Task PreReceive(ReceiveContext context) => Task.CompletedTask;
    public Task PostReceive(ReceiveContext context) => Task.CompletedTask;
    public Task ReceiveFault(ReceiveContext context, Exception exception)
    {
        Console.WriteLine($"[DLQ] Erro ao receber mensagem. Enviando para DLQ '{_dlqQueue}': {exception.Message}");
        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan elapsed, string consumerType, Exception exception) where T : class
    {
        Console.WriteLine($"[DLQ] Falha no consumo de {typeof(T).Name}. Enviando para DLQ '{_dlqQueue}': {exception.Message}");
        return Task.CompletedTask;
    }
}
