# Anti-Patterns & Common Pitfalls

---

## 1. Prohibited Anti-Patterns

### ❌ Anti-Pattern 1: Mutable Domain Events
```csharp
// BAD: Mutable event allows modification after dispatch
public class OrderCreated
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
}

// GOOD: Immutable record struct or sealed record with required IEvent properties
public readonly record struct OrderCreated(
    EventId Id,
    decimal Total,
    DateTimeOffset OccurredAt) : IDomainEvent;
```

### ❌ Anti-Pattern 2: Invoking External Brokers Inside Domain Handlers
```csharp
// BAD: In-process domain event handler directly publishes to Kafka/RabbitMQ
public sealed class OrderPlacedDomainEventHandler : IEventHandler<OrderPlacedDomainEvent>
{
    private readonly IKafkaProducer _kafkaProducer;

    public OrderPlacedDomainEventHandler(IKafkaProducer kafkaProducer) => _kafkaProducer = kafkaProducer;

    public async ValueTask HandleAsync(OrderPlacedDomainEvent eventInstance, CancellationToken ct)
    {
        await _kafkaProducer.SendAsync(eventInstance); // Breaks transactional consistency! Dual-write hazard!
    }
}

// GOOD: Persist events to the Transactional Outbox (e.g., via EricksonLopez.Outbox),
// and let an asynchronous relay process forward them to the broker with At-Least-Once guarantees.
```

