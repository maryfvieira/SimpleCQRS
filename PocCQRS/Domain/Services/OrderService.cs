using MassTransit;
using PocCQRS.Application.Commands;
using PocCQRS.Application.Events;
using PocCQRS.Infrastructure.Persistence.Repository;

namespace PocCQRS.Domain.Services;

public class OrderService: IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderService(IOrderRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> CreateOrderAsync(CreateOrder.Command command)
    {
        var orderId = await _repository.CreateOrderAsync(command.ProductName, command.Quantity);
        await _publishEndpoint.Publish(new OrderCreatedEvent(orderId, command.ProductName));
        return orderId;
    }

    public async Task<IResult> GetOrderAsync(Guid id)
    {
        var order = await _repository.GetOrderAsync(id);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }
}