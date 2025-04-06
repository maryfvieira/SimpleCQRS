using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PocCQRS.Domain.Events;

namespace PocCQRS.Infrastructure.Persistence;

 public class MongoEventStore : IEventStore
{
    private readonly IMongoCollection<EventDocument> _eventStore;

    public MongoEventStore(IMongoDatabase mongoDatabase)
    {
        _eventStore = mongoDatabase.GetCollection<EventDocument>("events");
        
        // Cria índice para melhorar consultas por AggregateId
        // var indexKeysDefinition = Builders<EventDocument>.IndexKeys.Ascending(e => e.AggregateId);
        // _eventStore.Indexes.CreateOne(new CreateIndexModel<EventDocument>(indexKeysDefinition));
        var indexKeys = Builders<EventDocument>.IndexKeys
            .Ascending(e => e.AggregateId)
            .Ascending(e => e.Version);
        _eventStore.Indexes.CreateOne(new CreateIndexModel<EventDocument>(indexKeys));
    }

    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion)
    {
        foreach (var @event in events)
        {
            var eventDocument = new EventDocument
            {
                EventId = @event.EventId,
                AggregateId = aggregateId,
                EventType = @event.GetType().FullName!,
                EventData = @event.ToBsonDocument(@event.GetType()),
                Version = ++expectedVersion,
                OccurredOn = @event.OcurredIn
            };

            await _eventStore.InsertOneAsync(eventDocument);
        }
    }

    public async Task<List<IDomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        var filter = Builders<EventDocument>.Filter.Eq(e => e.AggregateId, aggregateId);
        var sort = Builders<EventDocument>.Sort.Ascending(e => e.Version);
        
        var eventDocuments = await _eventStore.Find(filter)
            .Sort(sort)
            .ToListAsync();

        var events = new List<IDomainEvent>();
        foreach (var doc in eventDocuments)
        {
            var eventType = Type.GetType(doc.EventType);
            if (eventType != null && typeof(IDomainEvent).IsAssignableFrom(eventType))
            {
                var @event = (IDomainEvent)BsonSerializer.Deserialize(doc.EventData, eventType);
                events.Add(@event);
            }
        }

        return events;
    }
}


 
[BsonIgnoreExtraElements]
 public class EventDocument
 {
     [BsonId]
     [BsonRepresentation(BsonType.String)]
     public Guid EventId { get; set; }
     
     [BsonRepresentation(BsonType.String)]
     public Guid AggregateId { get; set; }

     public string EventType { get; set; } = default!;

     [BsonElement("EventData")]
     public BsonDocument EventData { get; set; } = default!;
     public int Version { get; set; }

     [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
     public DateTime OccurredOn { get; set; }
 }
