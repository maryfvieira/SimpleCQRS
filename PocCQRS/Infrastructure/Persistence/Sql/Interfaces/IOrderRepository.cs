using PocCQRS.Application.Queries;
using PocCQRS.Domain.Entities;

namespace PocCQRS.Infrastructure.Persistence.Sql.Interfaces;

public interface IOrderRepository
{
    Task<Guid> CreateAsync(Order order);
    Task<GetOrderQuery.Response?> GetOrderAsync(Guid orderId);
}