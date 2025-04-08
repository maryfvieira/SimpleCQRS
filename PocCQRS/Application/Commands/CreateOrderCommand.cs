using MediatR;

namespace PocCQRS.Application.Commands;

public record OrderItemCommand(int ProductId, int Quantity, double UnitPrice);
public record CreateOrderCommand(IList<OrderItemCommand> OrderItems) : IRequest<Guid>;