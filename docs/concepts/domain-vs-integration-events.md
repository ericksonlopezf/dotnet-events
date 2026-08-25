# Domain Events vs. Integration Events

## 1. Domain Events
A **Domain Event** represents something that happened inside a single Bounded Context / Aggregate Root.
- **Audience:** Internal to the Bounded Context.
- **Language:** Pure Ubiquitous Language (e.g. `OrderPlaced`, `InventoryReserved`).
- **Data Encapsulation:** Can reference domain-specific value objects, entity IDs, or internal structures.
- **Contract:** `IDomainEvent` interface (deriving from `IEvent`).

```csharp
public sealed record OrderPlaced(
    EventId Id,
    OrderId OrderId,
    CustomerId CustomerId,
    Money TotalAmount,
    DateTimeOffset OccurredAt) : IDomainEvent;
```

---

## 2. Integration Events
An **Integration Event** communicates that a notable business event occurred to other Bounded Contexts, microservices, or external consumers.
- **Audience:** External systems, decoupled microservices.
- **Language:** Stable public schema contract (e.g. `orders.order-placed.v1`).
- **Data Encapsulation:** Contains primitive types or well-defined, versioned DTOs. Never exposes internal aggregate entities or domain business logic.
- **Contract:** `IIntegrationEvent` interface (deriving from `IEvent`).

```csharp
[EventName("orders.order-placed")]
[EventVersion(1)]
public sealed record OrderPlacedIntegrationEvent(
    EventId Id,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
```

---

## 3. Mapping and the Outbox Pipeline

```text
┌────────────────────────────────────────────────────────────┐
│ Domain Aggregate (Order)                                  │
│  - Executes business logic                                │
│  - Raises: OrderPlaced (IDomainEvent)                     │
└─────────────────────────────┬──────────────────────────────┘
                              │
┌─────────────────────────────▼──────────────────────────────┐
│ Application Handler / Outbox Interceptor                   │
│  - Receives IDomainEvent                                   │
│  - Maps to: OrderPlacedIntegrationEvent (IIntegrationEvent)│
│  - Creates: EventEnvelope<OrderPlacedIntegrationEvent>     │
│  - Persists into Transactional Outbox                      │
└─────────────────────────────┬──────────────────────────────┘
                              │
┌─────────────────────────────▼──────────────────────────────┐
│ Outbox Publisher / Transport                               │
│  - Reads EventEnvelope from DB                             │
│  - Serializes JSON payload                                 │
│  - Publishes to Message Broker (Kafka, RabbitMQ)           │
└────────────────────────────────────────────────────────────┘
```
