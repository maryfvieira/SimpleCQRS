using Dapper;
using PocCQRS.Application.Queries;
using PocCQRS.Domain.Entities;
using PocCQRS.Infrastructure.Persistence.Sql.Interfaces;
using static MongoDB.Driver.WriteConcern;

namespace PocCQRS.Infrastructure.Persistence.Sql.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<Guid> CreateAsync(Order order)
    {
        using var connection = _connectionFactory.CreateConnection();
        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO Orders (Id, Quantity, Amount, Status, CreatedAt) 
                        VALUES (@Id, @Quantity, @Amount, @Status, @CreatedAt)",
                    new { Id = order.Id, Quantity = order.Quantity, Amount = order.Amount, Status = order.Status, CreatedAt = DateTime.UtcNow },
                    transaction
                );

                foreach (var orderItem in order.OrderItems)
                {
                    await connection.ExecuteAsync(
                        @"INSERT INTO OrderItem (OrderId, ProductId, Quantity, UnitPrice)
                            VALUES (@Id, @OrderId, @ProductId, @Quantity, @UnitPrice)",
                        new { Id = Guid.NewGuid(), OrderId = order.Id, ProductId = orderItem.ProductId, Quantity = orderItem.Quantity, UnitPrice = orderItem.UnitPrice },
                        transaction
                    );
                 }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();

                throw;
            }

        }
        return order.Id;   
    }

    public async Task<GetOrderQuery.Response?> GetOrderAsync(Guid orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<GetOrderQuery.Response>(
            "SELECT AggregateId, ProductId, Quantity, Amount FROM Orders WHERE AggregateId = @OrderId",
            new { OrderId = orderId }
        );
    }
}