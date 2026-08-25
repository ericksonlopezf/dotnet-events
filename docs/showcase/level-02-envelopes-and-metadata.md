# Level 02 — Event Envelopes & Distributed Metadata

In Level 02, we wrap domain events in structured `EventEnvelope<T>` wrappers to carry distributed context, correlation IDs, and causation chains.

---

## 1. The `EventEnvelope<T>` Structure

```csharp
using EricksonLopez.Events.Contracts;

var orderEvent = new OrderPlacedDomainEvent(
    OrderId: Guid.NewGuid(),
    CustomerId: Guid.NewGuid(),
    TotalAmount: 149.99m,
    OccurredOn: DateTimeOffset.UtcNow);

var envelope = EventEnvelope<OrderPlacedDomainEvent>.Create(
    payload: orderEvent,
    correlationId: "req-abc-123",
    causationId: "cmd-order-create",
    tenantId: "tenant-us-east-1");
```

---

## 2. Distributed Metadata Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Unique event envelope identifier |
| `EventType` | `string` | Canonical event type name for routing / serialization |
| `OccurredOn` | `DateTimeOffset` | UTC creation timestamp |
| `CorrelationId` | `string?` | End-to-end request identifier for distributed tracing |
| `CausationId` | `string?` | Identifier of the message/command that caused this event |
| `TenantId` | `string?` | Multi-tenant partition key |
| `Payload` | `T` | The typed domain or integration event instance |
