# Level 07 — Distributed Tracing & OpenTelemetry

In Level 07, we enrich event processing with distributed tracing, W3C context propagation, and runtime metrics using `EricksonLopez.Events.OpenTelemetry`.

---

## 1. Automatic Activity Tracing

Every event published via `IEventPublisher` automatically starts an OpenTelemetry `Activity`:

```csharp
using EricksonLopez.Events.OpenTelemetry;

services.AddEventsOpenTelemetry();
```

---

## 2. Distributed Context Propagation

The OpenTelemetry integration injects and extracts W3C `traceparent` and `tracestate` headers into `EventEnvelope<T>` metadata:

```csharp
// Inspect active activity tags
var activity = Activity.Current;
// Activity Name: "EricksonLopez.Events.Publish"
// Tag: "messaging.system" = "in-process"
// Tag: "messaging.destination" = "orders.placed.v1"
```
