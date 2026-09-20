# Features Catalog & Specifications

Exhaustive catalog of features, types, and architectural capabilities provided by `EricksonLopez.Events`.

---

## 1. Package Inventory & Capabilities

### 1. `EricksonLopez.Events.Contracts`
- `IDomainEvent`: Marker interface for immutable domain events contained within aggregate boundaries.
- `IIntegrationEvent`: Contract for cross-boundary distributed asynchronous events.
- `IEvent`: Foundational base contract defining canonical event typing (`Id`, `OccurredAt`).
- `IEventEnvelope<out T>`: Read-only covariant contract for structured event envelopes.
- `EventEnvelope<T>`: Universal, immutable reference type (`sealed record class`) carrying `Id`, `EventType`, `EventSource`, `OccurredAt`, `Payload`, and extensible `Metadata`.
- `IEventPublisher`: Primary contract for in-process asynchronous event dispatching (`PublishAsync<T>`).
- `IEventHandler<T>`: Strongly-typed event consumer contract (`ValueTask HandleAsync(T eventInstance, CancellationToken ct)`).
- `IEnvelopeEventHandler<T>`: Strongly-typed envelope consumer contract (`ValueTask HandleAsync(IEventEnvelope<T> envelope, CancellationToken ct)`).
- Attributes: `[EventName]`, `[EventSource]`, `[EventVersion]`.

### 2. `EricksonLopez.Events`
- `IEventBus`: Core event bus contract extending `IEventPublisher`.
- `EventBus`: Centralized in-process event orchestration engine with pipeline middleware support.
- Middleware Pipeline: `IEventMiddleware`, `EventMiddlewareDelegate`, `CausationDepthLimitMiddleware`.
- Scope & Invocation: `HandlerScopePolicy` (`Auto`, `CreatePerHandler`, `ReuseAmbientScope`), zero-allocation delegate dispatch via `HandlerInvoker<TEvent>`.
- Dependency Injection Extensions:
  - `services.AddEventBus(options => ...)`
  - `services.AddEventHandler<TEvent, THandler>(lifetime)`
  - `services.AddEnvelopeEventHandler<TEvent, THandler>(lifetime)`
  - `services.AddEventMiddleware<TMiddleware>(lifetime)`

### 3. `EricksonLopez.Events.CloudEvents`
- `CloudEvent`: Full CNCF CloudEvents v1.0 standard representation (`id`, `source`, `type`, `time`, `datacontenttype`, `dataschema`, `data`).
- `CloudEventsJsonSerializerOptionsExtensions`: Native AOT System.Text.Json serializer options extensions and converters for CloudEvents.
- `CloudEventExtensions`: Bidirectional mapping methods (`ToCloudEvent()`, `ToEventEnvelope()`) between `EventEnvelope<T>` and CNCF CloudEvents.

### 4. `EricksonLopez.Events.Generators`
- Roslyn Incremental Source Generator (`EventIncrementalGenerator`) for compile-time discovery and registry generation.
- Compile-time Roslyn Analyzers:
  - `ELE001`: Event immutability enforcement (`readonly record struct` or `sealed record`).
  - `ELE002`: Positive integer validation on `[EventVersion]`.
  - `ELE003`: Non-empty event name validation on `[EventName]`.
  - `ELE004`: Non-empty event source URI validation on `[EventSource]`.
  - `ELE005`: Architectural boundary check preventing domain events from leaking into integration contracts.

### 5. `EricksonLopez.Events.OpenTelemetry`
- Activity tracing via BCL `ActivitySource ("EricksonLopez.Events")`.
- Metrics counters for published events, dispatched handlers, and execution duration using `Meter ("EricksonLopez.Events")`.
- W3C `traceparent` and `tracestate` context propagation via envelope metadata.

### 6. `EricksonLopez.Events.Serialization.SystemTextJson`
- Source-generated `JsonSerializerContext` definitions for zero-reflection event serialization.
- Native AOT compliant polymorphic and envelope JSON converters (`EventEnvelopeJsonConverterFactory`).

### 7. `EricksonLopez.Events.Testing`
- `FakeEventPublisher`: High-throughput in-memory test double capturing published events and envelopes.
- Declarative assertion helpers for verifying event publication, counts, causation trees, and payload criteria.

