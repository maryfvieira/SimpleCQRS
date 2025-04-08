using PocCQRS.Domain.Entities;
using PocCQRS.Domain.Models;
using PocCQRS.Infrastructure.Persistence.Sql.Interfaces;

namespace PocCQRS.Domain.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CreateOrderAsync(OrderDto orderDto)
    {
        //TODO: usar autoMapper
        var orderItems = orderDto.OrderItems.Select(oi => new OrderItem(oi.Id, oi.OrderId, oi.ProductId, oi.Quantity, oi.UnitPrice));
        var order = new Order(orderDto.Id, orderDto.Quantity, orderDto.Amount, orderItems, orderDto.Status, orderDto.CreatedOn);

        var orderId = await _repository.CreateAsync(order);

        return await Task.FromResult(orderId);
    }

    public async Task<IResult> GetOrderAsync(Guid id)
    {
        var order = await _repository.GetOrderAsync(id);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }
}