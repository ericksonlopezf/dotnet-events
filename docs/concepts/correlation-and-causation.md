# Correlation and Causation Tracing

## 1. Concepts

In distributed event-driven systems, tracing the origin and progression of actions across asynchronous boundaries is vital for debugging, auditing, and observability:

- **`CorrelationId`**: Identifies the overall business transaction or end-to-end user request across all service boundaries. Remains constant from the initial HTTP request / command through all downstream events and commands.
- **`CausationId`**: Identifies the immediate direct cause of this specific event (e.g. the specific CommandId or previous EventId that triggered the current event).

```text
[HTTP Request] (CorrelationId = C1)
       │
       ▼
[Command: CreateOrder] (Id = CMD-1, CorrelationId = C1)
       │
       ▼
[Domain Event: OrderCreated] (Id = EVT-1, CorrelationId = C1, CausationId = CMD-1)
       │
       ▼
[Integration Event: OrderPlacedIntegrationEvent] (Id = EVT-2, CorrelationId = C1, CausationId = EVT-1)
       │
       ▼
[Downstream Consumer]
       │
       ▼
[Command: ReserveInventory] (Id = CMD-2, CorrelationId = C1, CausationId = EVT-2)
```

## 2. Using `EventMetadata`

`EventMetadata` provides first-class properties for `CorrelationId`, `CausationId`, `TenantId`, and `Source`:

```csharp
var metadata = EventMetadata.Create(
    correlationId: CorrelationId.New(),
    causationId: CausationId.From("CMD-1049"),
    source: "ordering-service",
    tenantId: TenantId.From("tenant-us-east")
);

var envelope = EventEnvelope.Create(integrationEvent, metadata);
```
