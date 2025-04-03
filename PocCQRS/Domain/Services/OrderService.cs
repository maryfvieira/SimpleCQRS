using MassTransit;
using PocCQRS.Application.Commands;
using PocCQRS.Application.Events;
using PocCQRS.Infrastructure.Messaging;
using PocCQRS.Infrastructure.Persistence.Repository;

namespace PocCQRS.Domain.Services;

public class OrderService: IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly Publisher<OrderCreatedEvent> _publisher;

    public OrderService(IOrderRepository repository, IPublisherFactory publisherFactory)
    {
        _repository = repository;
        _publisher = publisherFactory.CreatePublisher<OrderCreatedEvent>();
    }

    public async Task<Guid> CreateOrderAsync(CreateOrderCommand command)
    {
        var orderId = await _repository.CreateOrderAsync(command.ProductName, command.Quantity, command.Amount);
        await _publisher.PublishAsync(new OrderCreatedEvent(orderId, command.ProductName));
        return orderId;
    }

    public async Task<IResult> GetOrderAsync(Guid id)
    {
        var order = await _repository.GetOrderAsync(id);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }
}