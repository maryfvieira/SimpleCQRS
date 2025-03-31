using PocCQRS.Application.Queries;

namespace PocCQRS.Infrastructure.Persistence.Repository;

public interface IOrderRepository
{
    Task<Guid> CreateOrderAsync(string productName, int quantity);
    Task<GetOrderQuery.Response?> GetOrderAsync(Guid orderId);
}