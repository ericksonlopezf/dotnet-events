# 18. OBSERVABILITY & OPENTELEMETRY AUDIT

## 1. DISTRIBUTED TRACING (`ActivitySource`)
The `EricksonLopez.Events.OpenTelemetry` package defines canonical telemetry sources:
- **ActivitySource Name**: `"EricksonLopez.Events"`
- **Generated Spans**:
  - `EventPublisher.Publish`: Encompasses the publication lifecycle.
  - `EventHandler.Handle`: Encompasses individual handler execution.
- **Attribute Tags**:
  - `messaging.system`: `"in-memory"`
  - `messaging.destination`: Event type name (`EventType`).
  - `messaging.message_id`: `EventId`.
  - `messaging.correlation_id`: `CorrelationId`.

---

## 2. W3C CONTEXT PROPAGATION (`traceparent`)
Event metadata (`EventMetadata`) serializes W3C trace context headers (`traceparent` and `tracestate`), guaranteeing distributed traces remain unbroken across process boundaries via brokers or Outbox stores.