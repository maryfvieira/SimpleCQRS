using MediatR;
using PocCQRS.Domain.Entities;

namespace PocCQRS.Application.Commands
{
    public record AddOrderItemCommand(Guid OrderId, IEnumerable<OrderItemCommand> OrderItems) : IRequest;
}
