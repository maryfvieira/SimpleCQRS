using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PocCQRS.Domain;
using PocCQRS.Domain.Events;
using PocCQRS.Infrastructure.Persistence.NoSql.Interfaces;

namespace PocCQRS.Infrastructure.Persistence.NoSql.Repository;

public class MongoEventStore : IEventStore
{
    private readonly IMongoCollection<EventDocument> _eventStore;
    private readonly IMongoCollection<SnapshotDocument> _snapshotStore;

    public MongoEventStore(IMongoDatabase mongoDatabase)
    {
        _eventStore = mongoDatabase.GetCollection<EventDocument>("events");

        _snapshotStore = mongoDatabase.GetCollection<SnapshotDocument>("snapshot");

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

    public async Task SaveSnapshotAsync(Guid aggregateId, Guid lastEventId, ISnapshot snapshotData, int version, DateTime createdAt, DateTime lastUpdateAt)
    {
        var options = new UpdateOptions { IsUpsert = true };
        var filter = Builders<SnapshotDocument>.Filter.Eq(e => e.AggregateId, aggregateId);
        var updateData = Builders<SnapshotDocument>.Update
            .Set(sd => sd.AggregateId, aggregateId)
            .Set(sd => sd.LastEventId, lastEventId)
            .Set(sd => sd.SnapshotData, snapshotData.ToBsonDocument(snapshotData.GetType()))
            .Set(sd => sd.Version, version)
            .Set(sd => sd.CreateAt, createdAt)
            .Set(sd => sd.LastUpdateAt, lastUpdateAt);

        await _snapshotStore.UpdateOneAsync(filter, updateData, options);
    }

    public async Task<TAggregate?> LoadAggregateAsync<TAggregate, TAggregateState>(Guid aggregateId) 
        where TAggregate : AggregateRoot<TAggregateState>
        where TAggregateState : ISnapshot, new()
    {
        var filter = Builders<SnapshotDocument>.Filter.Eq(e => e.AggregateId, aggregateId);
        var sort = Builders<SnapshotDocument>.Sort.Descending(e => e.LastUpdateAt);

        var snapshotDocument = await _snapshotStore.Find(filter).Sort(sort).FirstOrDefaultAsync();
        
        if (snapshotDocument == null) return null;

        var aggregate = Activator.CreateInstance<TAggregate>();

        aggregate.AggregateId = snapshotDocument.AggregateId;
        aggregate.LastEventId = snapshotDocument.LastEventId;
        aggregate.Snapshot = (TAggregateState)(ISnapshot)BsonSerializer.Deserialize(snapshotDocument.SnapshotData, typeof(TAggregateState));
        aggregate.Version = snapshotDocument.Version;
        aggregate.CreateAt = snapshotDocument.CreateAt;
        aggregate.LastUpdateAt = snapshotDocument.LastUpdateAt;
          
        return await Task.FromResult(aggregate);
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

[BsonIgnoreExtraElements]
public class SnapshotDocument
{
    [BsonId]
    [BsonElement("AggregateId")]
    [BsonRepresentation(BsonType.String)]
    public Guid AggregateId { get; set; }

    [BsonElement("Snapshot")]
    public BsonDocument SnapshotData { get; set; } = default!;

    [BsonElement("Version")]
    public int Version { get; set; }

    [BsonElement("DomainEvents")]
    public BsonArray DomainEvents { get; set; } = new BsonArray();

    [BsonElement("LastEventId")]
    [BsonRepresentation(BsonType.String)]
    public Guid LastEventId { get; set; }

    [BsonElement("CreateAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreateAt { get; set; }

    [BsonElement("LastUpdateAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastUpdateAt { get; set; }
}

[BsonIgnoreExtraElements]
public class DeadLetterEventDocument
{
    [BsonId]
    [BsonElement("Id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string EventType { get; set; } = default!;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("EventData")]
    public BsonDocument EventData { get; set; } = default!;
}
