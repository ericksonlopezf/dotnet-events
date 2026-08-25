# Level 04 — CloudEvents v1.0 Integration

In Level 04, we convert `EventEnvelope<T>` to CNCF CloudEvents v1.0 specifications for interoperability across heterogeneous distributed systems using `EricksonLopez.Events.CloudEvents`.

---

## 1. CloudEvents Conversion

```csharp
using EricksonLopez.Events.CloudEvents;

var envelope = EventEnvelope<OrderPlacedDomainEvent>.Create(orderEvent);

// Convert to CloudEvent JSON specification
CloudEvent cloudEvent = envelope.ToCloudEvent(
    source: new Uri("https://orders.eshop.com/v1"),
    type: "com.eshop.orders.orderplaced");

string json = CloudEventSerializer.Serialize(cloudEvent);
```

---

## 2. Inbound CloudEvents Deserialization

```csharp
EventEnvelope<OrderPlacedDomainEvent> incomingEnvelope = 
    CloudEventSerializer.Deserialize<OrderPlacedDomainEvent>(json);
```
