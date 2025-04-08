using Dapper;
using PocCQRS.Application.Queries;
using PocCQRS.Infrastructure.Persistence.Sql.Interfaces;

namespace PocCQRS.Infrastructure.Persistence.Sql.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public OrderRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<Guid> CreateOrderAsync(string productName, int quantity, double amount)
    {
        using var connection = _connectionFactory.CreateConnection();
        var orderId = Guid.NewGuid();
        await connection.ExecuteAsync(
            "INSERT INTO Orders (AggregateId, ProductId, Quantity, CreatedAt, Amount) VALUES (@AggregateId, @ProductId, @Quantity, @CreatedAt, @Amount)",
            new { Id = orderId, ProductName = productName, Quantity = quantity, CreatedAt = DateTime.UtcNow, Amount = amount }
        );
        return orderId;
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