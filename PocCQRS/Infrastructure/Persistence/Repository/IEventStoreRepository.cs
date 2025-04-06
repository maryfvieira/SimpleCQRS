using PocCQRS.Domain;

namespace PocCQRS.Infrastructure.Persistence.Repository;

public interface IEventStoreRepository
{
    Task<OrderAggregate?> GetAsync(Guid orderId);
    Task SaveAsync(OrderAggregate order);

}