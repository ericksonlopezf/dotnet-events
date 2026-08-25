# Features Catalog & Specifications

Exhaustive catalog of features, types, and architectural capabilities provided by `EricksonLopez.Events`.

---

## 1. Package Inventory & Capabilities

### 1. `EricksonLopez.Events.Contracts`
- `IDomainEvent`: Marker interface for immutable domain events within aggregate boundaries.
- `IIntegrationEvent`: Contract for cross-boundary distributed asynchronous events.
- `IEvent`: Base contract defining canonical event typing.
- `EventEnvelope<T>`: Universal, immutable event wrapper carrying `Id`, `OccurredOn`, `EventType`, `CorrelationId`, `CausationId`, `TenantId`, and `Payload`.
- `IEventPublisher`: Primary contract for in-process asynchronous event publishing.
- `IEventHandler<T>`: Strongly-typed event consumer contract.

### 2. `EricksonLopez.Events`
- `InMemoryEventPublisher`: High-throughput, zero-allocation in-process event publisher.
- `EventBus`: Centralized event orchestration engine.
- Dependency Injection extensions: `services.AddEvents()`, `services.AddEventHandler<THandler, TEvent>()`.

### 3. `EricksonLopez.Events.CloudEvents`
- `CloudEvent`: Full CNCF CloudEvents v1.0 standard representation.
- `CloudEventSerializer`: System.Text.Json NativeAOT converter and formatter for CloudEvents.
- `EventEnvelopeExtensions.ToCloudEvent()`: Extension methods to convert envelopes to CloudEvents.

### 4. `EricksonLopez.Events.Generators`
- Roslyn Incremental Source Generator for compile-time event registries.
- Attributes: `[EventRegistry]`, `[RegisterEvent]`.

### 5. `EricksonLopez.Events.Outbox` & `EricksonLopez.Events.Inbox`
- `IOutboxStore`: Abstraction for saving and polling pending event envelopes from transactional database storage.
- `IInboxStore`: Abstraction for tracking processed event IDs to prevent duplicate message side-effects.

### 6. `EricksonLopez.Events.OpenTelemetry`
- Activity tracing via BCL `ActivitySource ("EricksonLopez.Events")`.
- Metrics counters for published events, dispatched handlers, and execution duration.

### 7. `EricksonLopez.Events.Serialization.SystemTextJson`
- Source-generated `JsonSerializerContext` definitions for zero-reflection event serialization.

### 8. `EricksonLopez.Events.Testing`
- `FakeEventPublisher`: In-memory test double capturing published envelopes with declarative assertion helpers.
