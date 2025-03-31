using MassTransit;
using PocCQRS.Application.Events;

namespace PocCQRS.EntryPoint.Consumer;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        Console.WriteLine($"[Consumer] Order created: {context.Message.OrderId} - {context.Message.ProductName}");
        return Task.CompletedTask;
    }
}