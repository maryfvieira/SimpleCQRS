using MassTransit;
using MediatR;
using PocCQRS.Application.Commands;
using PocCQRS.Domain;
using PocCQRS.Domain.Events;
using PocCQRS.Infrastructure.Messaging;
using PocCQRS.Infrastructure.Persistence.Cache.Interfaces;
using PocCQRS.Infrastructure.Persistence.NoSql.Interfaces;

namespace PocCQRS.Application.CommandHandlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    //private readonly IOrderService _orderService;
    private readonly Publisher<OrderCreatedEvent> _publisher;
    private readonly IEventStoreRepository _eventStoreRepository;
    private readonly ICacheClient _cacheClient;

    public CreateOrderCommandHandler(
       // IOrderService orderService,
       IPublisherFactory publisherFactory,
       IEventStoreRepository eventStoreRepository,
       ICacheClient cacheClient)
    {
        _publisher = publisherFactory.CreatePublisher<OrderCreatedEvent>();
        _eventStoreRepository = eventStoreRepository;
        _cacheClient = cacheClient;
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

        // TODO: Considerar a forte dependencia aos 4 metodos abaixo.
        //       Em caso um deles falhar, mesmo apos a resiliencia, deve-se dar rollback nos processos anteriores.
        
        // Mongo EventStore
        await _eventStoreRepository.SaveEventAsync(orderAggregate);

        // Mongo Snapshot
        await _eventStoreRepository.SaveSnapshotAsync<OrderAggregate, OrderSnapshot>(orderAggregate);

        // Redis
        await _cacheClient.SetAsync(orderAggregate.AggregateId.ToString("D"), orderAggregate);

        // Rabbit
        await _publisher.PublishAsync(@event);
        
        return @event.OrderId;
    }
}