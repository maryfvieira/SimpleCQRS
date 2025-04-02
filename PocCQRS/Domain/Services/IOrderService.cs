using PocCQRS.Application.Commands;

namespace PocCQRS.Domain.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(CreateOrderCommand command);
    Task<IResult> GetOrderAsync(Guid id);
}