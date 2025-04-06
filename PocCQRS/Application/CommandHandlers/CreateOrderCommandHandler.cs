using MassTransit;
using MediatR;
using PocCQRS.Application.Commands;
using PocCQRS.Domain;
using PocCQRS.Domain.Events;
using PocCQRS.Domain.Services;
using PocCQRS.Infrastructure.Messaging;
using PocCQRS.Infrastructure.Persistence.Repository;

namespace PocCQRS.Application.CommandHandlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    //private readonly IOrderService _orderService;
    private readonly Publisher<OrderCreatedEvent> _publisher;
    private readonly IEventStoreRepository _eventStoreRepository;

    public CreateOrderCommandHandler(
       // IOrderService orderService,
       IPublisherFactory publisherFactory,
        IEventStoreRepository eventStoreRepository)
    {
        _publisher = publisherFactory.CreatePublisher<OrderCreatedEvent>();
        _eventStoreRepository = eventStoreRepository;
    }

    public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // Persiste no MySQL via Dapper
        //var orderId = await _orderService.CreateOrderAsync(commad);
        var orderId = Guid.NewGuid();
        var orderItemsEvent = command.OrderItems.Select(oi => new OrderItemEvent(oi.ProductId, oi.UnitPrice, oi.Quantity));
        var @event = new OrderCreatedEvent(orderId, orderItemsEvent);
        
        var orderAggregate = new OrderAggregate();
        bool isCreated = await orderAggregate.CreateOrderAsync(orderId, @event);

        if (!isCreated) 
            return Guid.Empty;
        
        await _eventStoreRepository.SaveAsync(orderAggregate);
        await _publisher.PublishAsync(@event);
        
        return @event.OrderId;
    }
}