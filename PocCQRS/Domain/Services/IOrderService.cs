using PocCQRS.Application.Commands;

namespace PocCQRS.Domain.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(CreateOrder.Command command);
    Task<IResult> GetOrderAsync(Guid id);
}