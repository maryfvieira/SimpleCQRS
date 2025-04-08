using MediatR;
using PocCQRS.Application.Commands;
using PocCQRS.Domain;
using PocCQRS.Domain.Events;
using PocCQRS.Infrastructure.Messaging;
using PocCQRS.Infrastructure.Persistence.Cache.Interfaces;
using PocCQRS.Infrastructure.Persistence.NoSql.Interfaces;

namespace PocCQRS.Application.CommandHandlers
{
    public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand>
    {
        private readonly Publisher<OrderAddedItemEvent> _publisher;
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly ICacheClient _cacheClient;
        private readonly ILogger<AddOrderItemCommandHandler> _logger;

        public AddOrderItemCommandHandler(
            IPublisherFactory publisherFactory,
            IEventStoreRepository eventStoreRepository,
            ICacheClient cacheClient,
            ILogger<AddOrderItemCommandHandler> logger)
        {
            _publisher = publisherFactory.CreatePublisher<OrderAddedItemEvent>();
            _eventStoreRepository = eventStoreRepository;
            _cacheClient = cacheClient;
            _logger = logger;
        }

        public async Task Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
        {
            OrderAggregate? orderAggregate = null;

            var orderItemsEvent = request.OrderItems.Select(oi => new OrderItemEvent(oi.ProductId, oi.UnitPrice, oi.Quantity));
            var @event = new OrderAddedItemEvent(request.OrderId, orderItemsEvent);

            _cacheClient.TryGetValue(request.OrderId.ToString("D"), out orderAggregate);

            orderAggregate ??= await _eventStoreRepository.LoadAggregateAsync<OrderAggregate, OrderSnapshot>(request.OrderId);

            if (orderAggregate == null)
                throw new InvalidOperationException("Order was not created to be updated");

            orderAggregate.AddItem(@event);

            await _eventStoreRepository.SaveEventAsync(orderAggregate);

            await _eventStoreRepository.SaveSnapshotAsync<OrderAggregate, OrderSnapshot>(orderAggregate);

            await _cacheClient.SetAsync(request.OrderId.ToString("D"), orderAggregate);

            await _publisher.PublishAsync(@event);

            await Task.CompletedTask;
        }
    }
}