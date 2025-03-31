using Dapper;
using PocCQRS.Application.Queries;

namespace PocCQRS.Infrastructure.Persistence.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public OrderRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<Guid> CreateOrderAsync(string productName, int quantity)
    {
        using var connection = _connectionFactory.CreateConnection();
        var orderId = Guid.NewGuid();
        await connection.ExecuteAsync(
            "INSERT INTO Orders (Id, ProductName, Quantity, CreatedAt) VALUES (@Id, @ProductName, @Quantity, @CreatedAt)",
            new { Id = orderId, ProductName = productName, Quantity = quantity, CreatedAt = DateTime.UtcNow }
        );
        return orderId;
    }

    public async Task<GetOrderQuery.Response?> GetOrderAsync(Guid orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<GetOrderQuery.Response>(
            "SELECT Id, ProductName, Quantity FROM Orders WHERE Id = @OrderId",
            new { OrderId = orderId }
        );
    }
}