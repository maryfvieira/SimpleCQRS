using MassTransit;
using MediatR;
using PocCQRS.Application.Commands;
using PocCQRS.Application.Events;
using PocCQRS.Domain.Services;
using PocCQRS.Infrastructure.Messaging;
using PocCQRS.Infrastructure.Persistence.Repository;

namespace PocCQRS.Application.CommandHandlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderService _orderService;

    public CreateOrderCommandHandler(
        IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Persiste no MySQL via Dapper
        var orderId = await _orderService.CreateOrderAsync(request);
        return orderId;
    }
}