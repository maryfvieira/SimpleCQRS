using MassTransit;
using PocCQRS.Domain.Events;

namespace PocCQRS.EntryPoint.Consumer;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        string orderItems
            = string.Join(
                Environment.NewLine,
                context.Message.OrderItems
                    .Select(oi => $"Product: {oi.ProductId} - Quantity: {oi.Quantity} - Price: {oi.UnitPrice}"));
        Console.WriteLine(
            $"[Consumer] Order created: {Environment.NewLine} OrderId: {context.Message.OrderId} {Environment.NewLine} OrderItems: {orderItems}");
        
        return Task.CompletedTask;
    }
}