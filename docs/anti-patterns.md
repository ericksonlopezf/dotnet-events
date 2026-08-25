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

// GOOD: Immutable record struct
public readonly record struct OrderCreated(Guid Id, decimal Total) : IDomainEvent;
```

### ❌ Anti-Pattern 2: Invoking External Brokers Inside Domain Handlers
```csharp
// BAD: Domain event handler directly publishes to Kafka
public class DomainHandler : IEventHandler<OrderPlacedDomainEvent>
{
    public async Task HandleAsync(EventEnvelope<OrderPlacedDomainEvent> env, CancellationToken ct)
    {
        await _kafkaProducer.SendAsync(env.Payload); // Breaks transactional consistency!
    }
}

// GOOD: Save to Transactional Outbox, separate background worker pushes to broker
```
