using Dapper;
using PocCQRS.Application.Queries;

namespace PocCQRS.Infrastructure.Persistence.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public OrderRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<Guid> CreateOrderAsync(string productName, int quantity, double amount)
    {
        using var connection = _connectionFactory.CreateConnection();
        var orderId = Guid.NewGuid();
        await connection.ExecuteAsync(
            "INSERT INTO Orders (Id, ProductName, Quantity, CreatedAt, Amount) VALUES (@Id, @ProductName, @Quantity, @CreatedAt, @Amount)",
            new { Id = orderId, ProductName = productName, Quantity = quantity, CreatedAt = DateTime.UtcNow, Amount = amount }
        );
        return orderId;
    }

    public async Task<GetOrderQuery.Response?> GetOrderAsync(Guid orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<GetOrderQuery.Response>(
            "SELECT Id, ProductName, Quantity, Amount FROM Orders WHERE Id = @OrderId",
            new { OrderId = orderId }
        );
    }
}