# Level 05 — Transactional Outbox & Inbox Patterns

In Level 05, we implement reliable at-least-once event delivery and exactly-once idempotent consumption using `EricksonLopez.Events.Outbox` and `EricksonLopez.Events.Inbox`.

---

## 1. Persisting to the Transactional Outbox

When committing database business transactions, serialize domain events directly into the outbox table within the same transaction:

```csharp
using EricksonLopez.Events.Outbox;

public async Task CreateOrderAsync(Order order, CancellationToken ct)
{
    await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

    _dbContext.Orders.Add(order);
    
    foreach (var domainEvent in order.DomainEvents)
    {
        var envelope = EventEnvelope<IDomainEvent>.Create(domainEvent);
        await _outboxStore.SaveAsync(envelope, ct);
    }

    await _dbContext.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
}
```

---

## 2. Idempotent Inbound Processing with Inbox

```csharp
using EricksonLopez.Events.Inbox;

public async Task ProcessIncomingEventAsync(EventEnvelope<OrderPlacedDomainEvent> envelope, CancellationToken ct)
{
    if (await _inboxStore.HasBeenProcessedAsync(envelope.Id, ct))
    {
        // Duplicate message detected, safely skip
        return;
    }

    await _handler.HandleAsync(envelope, ct);
    await _inboxStore.MarkProcessedAsync(envelope.Id, ct);
}
```
