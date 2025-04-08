using PocCQRS.Domain.Models;

namespace PocCQRS.Domain.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(OrderDto orderDto);
    Task<IResult> GetOrderAsync(Guid id);
}