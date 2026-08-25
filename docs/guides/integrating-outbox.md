# Guide: Integrating EventEnvelope with EricksonLopez.Outbox

The Transactional Outbox pattern guarantees at-least-once delivery of domain and integration events without distributed 2-phase commits (XA transactions).

`EricksonLopez.Events` provides the immutable primitives (`EventEnvelope<TEvent>`, `EventId`, `EventMetadata`) that `EricksonLopez.Outbox` persists and forwards to message brokers.

---

## 1. Architectural Responsibility

```text
Domain / Application Layer:
   1. Aggregate creates IDomainEvent.
   2. Application Service maps to IIntegrationEvent.
   3. Wraps in EventEnvelope.Create(event, metadata).

EricksonLopez.Outbox:
   4. Persists serialized EventEnvelope to Outbox Table in same DB transaction.
   5. Background worker reads pending records from Outbox Table.
   6. Forwards wire payload & headers to Transport Adapter (Kafka, RabbitMQ, Azure Service Bus).
```

---

## 2. Wrapping Events with Ambient Metadata

When creating an outbox message, attach `CorrelationId`, `CausationId`, and `TenantId`:

```csharp
namespace MyApp.Application.Services;

using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using MyApp.Domain.Events;

public class OrderService
{
    private readonly IOutboxStore _outboxStore;

    public OrderService(IOutboxStore outboxStore)
    {
        _outboxStore = outboxStore;
    }

    public async Task CompleteOrderAsync(Guid orderId, decimal amount, string tenant, CancellationToken ct)
    {
        var @event = new OrderPlacedEvent(EventId.New(), orderId, amount, DateTimeOffset.UtcNow);

        var metadata = new EventMetadataBuilder()
            .WithCorrelationId(CorrelationId.New())
            .WithCausationId(CausationId.From($"CMD-ORDER-{orderId}"))
            .WithTenantId(TenantId.From(tenant))
            .WithSource("ordering.service")
            .WithHeader("X-Environment", "Production")
            .Build();

        var envelope = EventEnvelope.Create(@event, metadata);

        // Store in transactional outbox
        await _outboxStore.SaveAsync(envelope, ct);
    }
}
```

---

## 3. Serialization for the Outbox

`EricksonLopez.Events.Serialization.SystemTextJson` provides custom converters for `EventEnvelope<TEvent>` and all primitives.

```csharp
using System.Text.Json;
using EricksonLopez.Events.Serialization.SystemTextJson;

var options = new JsonSerializerOptions();
options.AddEventsConverters();

// Serializes the full envelope (Id, Type, Version, OccurredAt, Metadata, Payload)
string outboxJsonPayload = JsonSerializer.Serialize(envelope, options);
```

---

## 4. Multi-Tenant Outbox Routing

Because `TenantId` is a first-class property of `EventMetadata` (see ADR-024):
- The Outbox poller can partition work by tenant.
- Messages can be routed to tenant-specific broker partitions or queues without parsing custom header dictionaries.
