using MassTransit;
using MediatR;
using PocCQRS.Application.Commands;
using PocCQRS.Application.Events;
using PocCQRS.Infrastructure.Persistence.Repository;

namespace PocCQRS.Application.CommandHandlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Persiste no MySQL via Dapper
        var orderId = await _orderRepository.CreateOrderAsync(request.ProductName, request.Quantity);
        
        // Publica evento via MassTransit
        await _publishEndpoint.Publish(
            new OrderCreatedEvent(orderId, request.ProductName), 
            cancellationToken
        );

        return orderId;
    }
}