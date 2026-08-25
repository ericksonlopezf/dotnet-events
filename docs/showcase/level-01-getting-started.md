# Level 01 — Getting Started & Event Primitives

In Level 01, we define domain event contracts and explore fundamental event creation using `EricksonLopez.Events.Contracts`.

---

## 1. Defining a Domain Event

All domain events implement `IDomainEvent` or `IEvent`.

```csharp
using EricksonLopez.Events.Contracts;

// Immutable domain event representing an order placement
public sealed record OrderPlacedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTimeOffset OccurredOn) : IDomainEvent;
```

---

## 2. Defining an Integration Event

Integration events are cross-service contracts intended for external asynchronous communication:

```csharp
public sealed record OrderShippedIntegrationEvent(
    Guid OrderId,
    string TrackingNumber,
    DateTimeOffset ShippedAt) : IIntegrationEvent;
```

---

## 3. Best Practices for Event Payloads

- **Keep payloads minimal & cohesive**: Include state identifiers, event timestamps, and immutable deltas.
- **Never mutate domain events after instantiation**: Enforce `init` or positional record parameters.
- **Ensure NativeAOT compatibility**: Avoid polymorphic inheritance hierarchies in event payloads.
