using MediatR;

namespace PocCQRS.Application.Commands;

public record CreateOrderCommand(string ProductName, int Quantity) : IRequest<Guid>;