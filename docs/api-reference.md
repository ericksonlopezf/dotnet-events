# API Reference — EricksonLopez.Events (Microsoft Learn Style)

Technical documentation and exhaustive specification of all public types, interfaces, structures, records, builders, and extension methods that make up the **EricksonLopez.Events** ecosystem.

---

## Assembly Index

- [1. EricksonLopez.Events.Contracts](#1-ericksonlopezeventscontracts)
  - [Event and Handler Interfaces](#event-and-handler-interfaces)
  - [Strongly Typed Identifiers](#strongly-typed-identifiers)
  - [Metadata and Builders](#metadata-and-builders)
  - [Declarative Attributes](#declarative-attributes)
- [2. EricksonLopez.Events](#2-ericksonlopezevents)
  - [EventBus and Dependency Injection](#eventbus-and-dependency-injection)
  - [Execution Strategies and Options](#execution-strategies-and-options)
  - [Middlewares and Pipeline](#middlewares-and-pipeline)
  - [EventEnvelope and Registry](#eventenvelope-and-registry)
  - [Diagnostics and Telemetry](#diagnostics-and-telemetry)
- [3. EricksonLopez.Events.Serialization.SystemTextJson](#3-ericksonlopezeventsserializationsystemtextjson)
- [4. EricksonLopez.Events.Generators](#4-ericksonlopezeventsgenerators)
- [5. EricksonLopez.Events.CloudEvents](#5-ericksonlopezeventscloudevents)
- [6. EricksonLopez.Events.OpenTelemetry](#6-ericksonlopezeventsopentelemetry)
- [7. EricksonLopez.Events.Testing](#7-ericksonlopezeventstesting)
- [8. Public API Master Inventory (Source of Truth)](#8-public-api-master-inventory-source-of-truth)

---

## 8. Public API Master Inventory (Source of Truth)

The following table constitutes the **exhaustive, single source of truth** for all public elements exposed by the Core Library and Infrastructure assemblies. No showcase example or cookbook recipe may utilize APIs outside this inventory.

| Type / Member | Namespace | Responsibility | Dependencies | Use Cases | Complexity | Existing Example |
|---|---|---|---|---|---|---|
| `IEvent` | `EricksonLopez.Events.Contracts` | Base event contract in ecosystem. Exposes `Id` (GUID v7) and `OccurredAt`. | `EventId` | Foundation for domain and integration events. | Basic | Yes (L1) |
| `IDomainEvent` | `EricksonLopez.Events.Contracts` | Marker interface for internal domain events within bounded contexts. | `IEvent` | Internal emission in DDD Aggregate Roots. | Basic | Yes (L1) |
| `IIntegrationEvent` | `EricksonLopez.Events.Contracts` | Marker interface for events crossing context or service boundaries. | `IEvent` | Publication for distributed asynchronous services. | Basic | Yes (L1, L2) |
| `IEventHandler<in TEvent>` | `EricksonLopez.Events.Contracts` | High-performance typed asynchronous subscriber contract returning `ValueTask`. | `IEvent` | Subscription to domain and integration events. | Basic | Yes (L3, L4) |
| `IEnvelopeEventHandler<in TEvent>` | `EricksonLopez.Events.Contracts` | Contract for subscribers requiring direct access to `IEventEnvelope<T>` and metadata. | `IEvent`, `IEventEnvelope<T>` | Auditing, partition routing, and tenant context access. | Intermediate | Yes (L4, L11) |
| `IEventPublisher` | `EricksonLopez.Events.Contracts` | Decoupled in-process event publication contract. | `IEvent` | Injection dependency for Application Services. | Basic | Yes (L3, L4) |
| `IEventSubscriber` | `EricksonLopez.Events.Contracts` | Contract for dynamic in-memory subscription and unsubscription. | `IEvent`, `IEventHandler<T>` | Unit testing and lightweight in-memory buses. | Intermediate | Yes (L3) |
| `IEventBus` | `EricksonLopez.Events.Contracts` | Unified in-process event bus (inherits from `IEventPublisher`). | `IEventPublisher` | Primary orchestration of in-process dispatch in DI. | Basic | Yes (L4) |
| `IEventEnvelope` | `EricksonLopez.Events.Envelopes` | Non-generic interface for polymorphic event envelope access. | `EventId`, `EventType`, `EventVersion`, `EventMetadata` | Heterogeneous serialization and middleware pipelines. | Intermediate | Yes (L2, L11) |
| `IEventEnvelope<out TEvent>` | `EricksonLopez.Events.Envelopes` | Covariant generic interface for typed access to payload and metadata. | `IEventEnvelope`, `IEvent` | Handlers needing metadata and transport adapters. | Intermediate | Yes (L2, L4) |
| `EventEnvelope` | `EricksonLopez.Events.Envelopes` | Static factory class with `Create` and `Wrap` helper methods. | `IEvent`, `EventMetadata`, `EventEnvelope<T>` | Convenient wrapping of domain event payloads. | Basic | Yes (L2) |
| `EventEnvelope<TEvent>` | `EricksonLopez.Events.Envelopes` | Immutable reference-type record encapsulating payload and metadata. | `IEventEnvelope<T>`, `EventMetadata` | Canonical envelope for dispatch, outbox, and serialization. | Intermediate | Yes (L2, L10) |
| `EventMetadata` | `EricksonLopez.Events.Metadata` | Contextual metadata model (CorrelationId, CausationId, TenantId, Headers). | `CorrelationId`, `CausationId`, `TenantId` | Distributed context propagation and observability. | Basic | Yes (L2) |
| `EventMetadataBuilder` | `EricksonLopez.Events.Metadata` | Fluent builder for immutable `EventMetadata` construction. | `EventMetadata`, Identifiers | Low-allocation contextual metadata creation. | Basic | Yes (L2) |
| `EventContext` | `EricksonLopez.Events.Context` | Ambient context (`AsyncLocal`) for metadata and active execution tracker. | `IEventEnvelope`, `EventMetadata`, `IEventExecutionTracker` | Ambient access to Tenant and CorrelationId in handlers. | Intermediate | Yes (L4, L11) |
| `IEventExecutionTracker` | `EricksonLopez.Events.Context` | Handler execution tracker during transactional event dispatch. | `EventId` | Idempotency and transactional commit coordination. | Advanced | Yes (L11) |
| `EventId` | `EricksonLopez.Events.Identifiers` | Immutable GUID v7 value-type struct with chronological B-Tree sorting. | BCL (`Guid`, `ISpanFormattable`, `IParsable`) | Unique event identity with monotonic time ordering. | Basic | Yes (L1, L7) |
| `EventType` | `EricksonLopez.Events.Identifiers` | Immutable string-backed value type decoupled from CLR namespaces. | BCL (`IComparable`, `IEquatable`) | Canonical semantic event naming for routing and serialization. | Basic | Yes (L1, L5) |
| `EventVersion` | `EricksonLopez.Events.Identifiers` | Immutable monotonic version identifier (`uint >= 1`). | BCL (`IComparable`, `IEquatable`) | Schema evolution and contract versioning. | Basic | Yes (L1, L5) |
| `CorrelationId` | `EricksonLopez.Events.Identifiers` | Immutable struct identifier for end-to-end distributed traceability. | BCL (`IComparable`, `IEquatable`) | Grouping all operations within a business interaction. | Basic | Yes (L2) |
| `CausationId` | `EricksonLopez.Events.Identifiers` | Immutable struct identifying direct cause of event emission. | BCL (`IComparable`, `IEquatable`) | Causal graph construction and infinite loop prevention. | Basic | Yes (L2, L8) |
| `TenantId` | `EricksonLopez.Events.Identifiers` | Immutable struct for multi-tenant partition routing and isolation. | BCL (`IComparable`, `IEquatable`) | Message routing and partition segregation per tenant. | Basic | Yes (L2, L11) |
| `EventNameAttribute` | `EricksonLopez.Events.Attributes` | Declarative attribute assigning semantic event name (`EventType`). | BCL (`Attribute`) | Compile-time mapping for Source Generators and catalogs. | Basic | Yes (L1, L5) |
| `EventVersionAttribute` | `EricksonLopez.Events.Attributes` | Declarative attribute specifying contract schema version. | BCL (`Attribute`) | Schema compatibility control and event evolution. | Basic | Yes (L1, L5) |
| `EventSourceAttribute` | `EricksonLopez.Events.Attributes` | Declarative attribute identifying publishing system or URI. | BCL (`Attribute`) | CNCF CloudEvents metadata and distributed tracing. | Basic | Yes (L1, L9) |
| `EventBus` | `EricksonLopez.Events.Bus` | Reference implementation of `IEventBus` with pipeline and DI support. | `IExecutionStrategy`, `IHandlerRegistry`, `IEventMiddleware` | Production in-process decoupled event dispatching. | Intermediate | Yes (L4) |
| `EventBusOptions` | `EricksonLopez.Events.Bus.Configuration` | Configuration options for bus execution mode, reentrancy, and errors. | `EventExecutionMode`, `ErrorHandlingPolicy`, `HandlerScopePolicy` | Performance tuning and resilience policies for the bus. | Intermediate | Yes (L4, L5, L6) |
| `EventExecutionMode` | `EricksonLopez.Events.Bus.Configuration` | Enum defining dispatch execution mode: `Sequential` or `Parallel`. | None | Handler concurrency strategy per event. | Intermediate | Yes (L5) |
| `ErrorHandlingPolicy` | `EricksonLopez.Events.Bus.Configuration` | Enum defining error policy: `FailFast` or `AggregateAndContinue`. | None | Resilience against partial handler failures. | Intermediate | Yes (L6) |
| `HandlerScopePolicy` | `EricksonLopez.Events.Bus.Configuration` | Enum for DI scope resolution: `Auto`, `CreatePerHandler`, `ReuseAmbientScope`. | None | Scoped service and DbContext isolation in parallel runs. | Advanced | Yes (L11) |
| `EventBusDiagnostics` | `EricksonLopez.Events.Bus.Diagnostics` | Static telemetry metrics class for event bus dispatch operations. | BCL | Latency and throughput instrumentation for event bus. | Advanced | Yes (L11) |
| `EventDispatchException` | `EricksonLopez.Events.Bus.Exceptions` | Aggregate exception thrown when one or more handlers fail. | BCL (`Exception`), `EventType` | Error diagnostic aggregation in resilient policies. | Intermediate | Yes (L6) |
| `EventTypeNotFoundException` | `EricksonLopez.Events.Exceptions` | Exception thrown when an event type is not registered in the catalog. | BCL (`Exception`), `EventType` | Guard against unmapped or untyped event messages. | Intermediate | Yes (L6, L11) |
| `EventValidationException` | `EricksonLopez.Events.Exceptions` | Exception thrown when event structure or payload validation fails. | BCL (`Exception`) | Semantic validation enforcement in middlewares. | Intermediate | Yes (L11) |
| `IExecutionStrategy` | `EricksonLopez.Events.Bus.Execution` | Strategy contract for invoking registered handler descriptors. | `HandlerDescriptor`, `EventBusOptions` | Customization of concurrency and execution behavior. | Advanced | Yes (L8, L11) |
| `SequentialExecutionStrategy` | `EricksonLopez.Events.Bus.Execution` | Sequential dispatch strategy reusing ambient DI scope. | `IExecutionStrategy` | Default deterministic and transactional execution mode. | Advanced | Yes (L5, L11) |
| `ParallelExecutionStrategy` | `EricksonLopez.Events.Bus.Execution` | Concurrent dispatch strategy via `Task.WhenAll` with isolated scopes. | `IExecutionStrategy` | High-throughput dispatch for non-blocking I/O handlers. | Advanced | Yes (L5) |
| `EventBusServiceCollectionExtensions` | `EricksonLopez.Events.Bus.Extensions` | Extension methods for registering EventBus and handlers in DI. | `IServiceCollection` | Clean DI configuration in `Program.cs` / `Startup`. | Basic | Yes (L4, L11) |
| `HandlerRegistrationToken` | `EricksonLopez.Events.Bus.Extensions` | Token transferring handler descriptor metadata to singleton registry. | `HandlerDescriptor` | Internal reflection-free DI wiring mechanism. | Advanced | Yes (L11) |
| `IEventMiddleware` | `EricksonLopez.Events.Bus.Middleware` | Interceptor contract for the event dispatch pipeline. | `EventMiddlewareDelegate<T>` | Cross-cutting logging, metrics, and reentrancy guards. | Intermediate | Yes (L8) |
| `EventMiddlewareDelegate<TEvent>` | `EricksonLopez.Events.Bus.Middleware` | Delegate representing the next step in the middleware chain. | `ValueTask` | Low-allocation asynchronous pipeline chaining. | Intermediate | Yes (L8) |
| `MiddlewarePipeline` | `EricksonLopez.Events.Bus.Middleware` | Static helper compiling the middleware delegate chain. | `IEventMiddleware`, `EventMiddlewareDelegate<T>` | Lightweight pipeline compilation without heavy overhead. | Advanced | Yes (L8) |
| `CausationDepthLimitMiddleware` | `EricksonLopez.Events.Bus.Middleware` | Built-in middleware preventing runaway cascading event recursion. | `IEventMiddleware` | Runtime safeguard against infinite event loops. | Intermediate | Yes (L8, L11) |
| `IHandlerRegistry` | `EricksonLopez.Events.Bus.Registry` | Contract for querying registered handlers by event type. | `HandlerDescriptor` | Precompiled handler descriptor catalog lookup. | Advanced | Yes (L11) |
| `HandlerRegistry` | `EricksonLopez.Events.Bus.Registry` | Thread-safe `IHandlerRegistry` implementation using concurrent index. | `IHandlerRegistry` | Thread-safe store of precompiled invocation descriptors. | Advanced | Yes (L11) |
| `HandlerDescriptor` | `EricksonLopez.Events.Bus.Registry` | Precompiled invocation descriptor with reflection-free invoker delegate. | BCL (`Func<object,object,ct,ValueTask>`) | Native AOT execution without `MethodInfo.Invoke`. | Advanced | Yes (L8, L11) |
| `HandlerInvoker<in TEvent>` | `EricksonLopez.Events.Bus.Registry` | Strongly typed invocation delegate preventing value-type boxing. | BCL (`ValueTask`) | High-performance dispatch without boxing overhead. | Advanced | Yes (L11) |
| `EventPublisherExtensions` | `EricksonLopez.Events.Contracts` | Extension methods for publishing `IEventEnvelope<T>` instances. | `IEventPublisher`, `IEventEnvelope<T>` | Direct envelope dispatch with enriched metadata. | Basic | Yes (L4) |
| `InMemoryEventPublisher` | `EricksonLopez.Events.Dispatch` | Lightweight in-memory bus with subscription and cancellation support. | `IEventBus`, `IEventSubscriber` | Unit testing, CLI tools, and rapid local prototyping. | Basic | Yes (L3) |
| `EventsDiagnostics` | `EricksonLopez.Events.Diagnostics` | Standard telemetry using OpenTelemetry `ActivitySource` and `Meter`. | System.Diagnostics | W3C distributed tracing and real-time metrics. | Intermediate | Yes (L9, L11) |
| `EventTypeDescriptor` | `EricksonLopez.Events.Registry` | Immutable descriptor pairing CLR Type, EventType, and Version. | `EventType`, `EventVersion` | Bidirectional mapping between semantic names and types. | Intermediate | Yes (L5) |
| `IEventTypeRegistry` | `EricksonLopez.Events.Registry` | Centralized catalog of registered event types. | `EventTypeDescriptor` | Polymorphic message routing and CloudEvents resolution. | Intermediate | Yes (L5) |
| `EventTypeRegistry` | `EricksonLopez.Events.Registry` | Immutable implementation of `IEventTypeRegistry`. | `IEventTypeRegistry` | Frozen, thread-safe event type catalog. | Intermediate | Yes (L5) |
| `EventTypeRegistryBuilder` | `EricksonLopez.Events.Registry` | Fluent builder for event type and assembly registration. | `EventTypeDescriptor` | Application startup registration builder. | Intermediate | Yes (L5) |
| `StaticEventTypeRegistry` | `EricksonLopez.Events.Registry` | Static facade with generic cache for zero-lock O(1) resolution. | `IEventTypeRegistry`, `EventTypeDescriptor` | Ultra-fast Native AOT dispatch on hot paths. | Advanced | Yes (L5, L11) |
| `CloudEvent<TData>` | `EricksonLopez.Events.CloudEvents` | Immutable record adhering to CNCF CloudEvents v1.0 specification. | BCL (`Uri`, `DateTimeOffset`) | Standard event exchange format across event meshes. | Intermediate | Yes (L9) |
| `CloudEventExtensions` | `EricksonLopez.Events.CloudEvents` | Bidirectional mapping between `EventEnvelope<T>` and `CloudEvent<T>`. | `EventEnvelope<T>`, `CloudEvent<T>` | Conversion adapters for CNCF brokers and gateways. | Intermediate | Yes (L9) |
| `CloudEventsJsonSerializerOptionsExtensions` | `EricksonLopez.Events.CloudEvents.Serialization` | Configures `JsonSerializerOptions` for CloudEvents serialization. | `JsonSerializerOptions` | Native AOT CloudEvents JSON formatting. | Intermediate | Yes (L9) |
| `EventIncrementalGenerator` | `EricksonLopez.Events.Generators` | Roslyn incremental source generator for compile-time AOT registries. | Roslyn SDK | Zero-reflection compile-time code generation. | Advanced | Yes (L10) |
| `DomainEventLeakAnalyzer` | `EricksonLopez.Events.Generators.Analyzers` | Analyzer ELE005: Prevents `IDomainEvent` from leaking into integration contracts. | Roslyn SDK | Enforces Clean Architecture / DDD boundary rules. | Advanced | Yes (L10) |
| `EventAttributeValidationAnalyzer` | `EricksonLopez.Events.Generators.Analyzers` | Analyzer ELE002/ELE003/ELE004: Validates version >= 1, non-empty names/sources. | Roslyn SDK | Prevents contract configuration errors at compile time. | Advanced | Yes (L10) |
| `EventImmutabilityAnalyzer` | `EricksonLopez.Events.Generators.Analyzers` | Analyzer ELE001: Enforces event immutability (`record` / `readonly`). | Roslyn SDK | Guarantees state immutability in concurrent dispatch. | Advanced | Yes (L10) |
| `EventsOpenTelemetryExtensions` | `EricksonLopez.Events.OpenTelemetry` | Extensions for TracerProviderBuilder and MeterProviderBuilder. | OpenTelemetry SDK | Automatic OpenTelemetry tracing and metrics hookup. | Intermediate | Yes (L9, L11) |
| `EventsJsonSerializerOptionsExtensions` | `EricksonLopez.Events.Serialization.SystemTextJson` | Methods configuring JsonSerializerOptions with Native AOT converters. | `JsonSerializerOptions` | Native AOT System.Text.Json configuration. | Intermediate | Yes (L10) |
| `EventIdJsonConverter` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | JSON converter for `EventId` (GUID string). | `JsonConverter<EventId>` | Low-allocation identifier serialization. | Advanced | Yes (L10) |
| `EventTypeJsonConverter` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | JSON converter for `EventType` (semantic string). | `JsonConverter<EventType>` | Semantic event name serialization. | Advanced | Yes (L10) |
| `EventVersionJsonConverter` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | JSON converter for `EventVersion` (uint integer). | `JsonConverter<EventVersion>` | Contract version serialization. | Advanced | Yes (L10) |
| `CorrelationIdJsonConverter` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | JSON converter for `CorrelationId` (string). | `JsonConverter<CorrelationId>` | Distributed correlation ID serialization. | Advanced | Yes (L10) |
| `CausationIdJsonConverter` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | JSON converter for `CausationId` (string). | `JsonConverter<CausationId>` | Distributed causation ID serialization. | Advanced | Yes (L10) |
| `TenantIdJsonConverter` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | JSON converter for `TenantId` (string). | `JsonConverter<TenantId>` | Multi-tenant identifier serialization. | Advanced | Yes (L10) |
| `EventMetadataJsonConverter` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | JSON converter for `EventMetadata` with frozen headers. | `JsonConverter<EventMetadata>` | Contextual metadata serialization. | Advanced | Yes (L10) |
| `EventEnvelopeJsonConverter<TEvent>` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | Generic converter for typed `EventEnvelope<T>` serialization. | `JsonConverter<EventEnvelope<T>>` | Typed envelope serialization without reflection. | Advanced | Yes (L11) |
| `EventEnvelopeJsonConverterFactory` | `EricksonLopez.Events.Serialization.SystemTextJson.Converters` | JSON converter factory for arbitrary `EventEnvelope<T>` types. | `JsonConverterFactory` | Polymorphic envelope registration in JsonSerializerOptions. | Advanced | Yes (L11) |
| `FakeEventPublisher` | `EricksonLopez.Events.Testing` | In-memory test double with fluent assertions and fault simulation. | `IEventPublisher` | Declarative unit testing without mocking frameworks. | Intermediate | Yes (L9, L11) |
| `TestEventHandler<TEvent>` | `EricksonLopez.Events.Testing` | Spy handler tracking invocations, delays, and simulated errors. | `IEventHandler<TEvent>` | Behavioral testing and asynchronous pipeline validation. | Intermediate | Yes (L9) |
| `EventTestBuilder` | `EricksonLopez.Events.Testing` | Static entry point instantiating `EventEnvelopeTestBuilder<T>`. | `EventEnvelopeTestBuilder<T>` | Fluent factory for test event envelopes. | Intermediate | Yes (L9) |
| `EventEnvelopeTestBuilder<TEvent>` | `EricksonLopez.Events.Testing` | Fluent builder creating synthetic `EventEnvelope<T>` test fixtures. | `EventEnvelope<T>`, `EventMetadata` | Fast construction of test event envelopes. | Intermediate | Yes (L9) |

---

## 1. EricksonLopez.Events.Contracts

Core assembly that defines the fundamental contracts, dispatch interfaces, and immutable GUID v7-based identifiers.

### Event and Handler Interfaces

#### `IEvent`
Base contract of the ecosystem. Every event must implement this interface.
```csharp
namespace EricksonLopez.Events.Contracts;

public interface IEvent
{
    EventId Id { get; }
    DateTimeOffset OccurredAt { get; }
}
```
- **Properties**:
  - `Id`: Unique 16-byte identifier based on GUID Version 7 (`EventId`).
  - `OccurredAt`: UTC timestamp (`DateTimeOffset`) at which the business fact occurred.

#### `IDomainEvent`
Marker interface for domain events emitted internally by an Aggregate Root within the same bounded context.
```csharp
namespace EricksonLopez.Events.Contracts;

public interface IDomainEvent : IEvent { }
```

#### `IIntegrationEvent`
Marker interface for integration events published across bounded context boundaries or distributed services.
```csharp
namespace EricksonLopez.Events.Contracts;

public interface IIntegrationEvent : IEvent { }
```

#### `IEventHandler<in TEvent>`
Asynchronous contract for event handlers.
```csharp
namespace EricksonLopez.Events.Contracts;

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    ValueTask HandleAsync(TEvent eventInstance, CancellationToken cancellationToken = default);
}
```
- **Parameters**:
  - `eventInstance`: The event instance to process.
  - `cancellationToken`: Cooperative cancellation token.
- **Returns**: High-performance `ValueTask` to avoid allocations on synchronous or fast-completing paths.

#### `IEventPublisher`
Contract for decoupled in-process publishing.
```csharp
namespace EricksonLopez.Events.Contracts;

public interface IEventPublisher
{
    ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
```

#### `IEventSubscriber`
Contract for manual in-memory subscriber registration (ideal for tests and lightweight scenarios).
```csharp
namespace EricksonLopez.Events.Contracts;

public interface IEventSubscriber
{
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
}
```

#### `IEventBus`
Unified interface for the in-process event bus. Inherits from `IEventPublisher`.
```csharp
namespace EricksonLopez.Events.Contracts;

public interface IEventBus : IEventPublisher { }
```

---

### Strongly Typed Identifiers

All identifiers implement `readonly record struct` to guarantee immutability, value-based comparison, and zero heap allocations.

#### `EventId`
Immutable identifier based on GUID Version 7 (RFC 9562), chronologically sortable and optimized for database indexes.
```csharp
namespace EricksonLopez.Events.Identifiers;

public readonly record struct EventId : IComparable<EventId>, IEquatable<EventId>, ISpanFormattable, IUtf8SpanFormattable, IParsable<EventId>, ISpanParsable<EventId>
{
    public Guid Value { get; }
    public static EventId New();
    public static EventId From(Guid value);
    public static EventId Parse(string s, IFormatProvider? provider = null);
    public static bool TryParse(string? s, IFormatProvider? provider, out EventId result);
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null);
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null);
}
```

#### `EventType`
Semantic name for the event type (decoupled from the CLR class name).
```csharp
namespace EricksonLopez.Events.Identifiers;

public readonly record struct EventType : IComparable<EventType>, IEquatable<EventType>, IParsable<EventType>
{
    public string Value { get; }
    public static EventType From(string value);
}
```

#### `EventVersion`
Monotonically increasing integer schema version (`uint >= 1`).
```csharp
namespace EricksonLopez.Events.Identifiers;

public readonly record struct EventVersion : IComparable<EventVersion>, IEquatable<EventVersion>, IParsable<EventVersion>
{
    public uint Value { get; }
    public static readonly EventVersion V1;
    public static EventVersion From(uint value);
}
```

#### `CorrelationId`, `CausationId`, `TenantId`
Value structures for distributed traceability and multitenant support:
```csharp
namespace EricksonLopez.Events.Identifiers;

public readonly record struct CorrelationId(string Value);
public readonly record struct CausationId(string Value);
public readonly record struct TenantId(string Value);
```
- `CorrelationId.Value`: `string` — a logical correlation chain ID (e.g. `"trace-abc-123"`).
- `CausationId.Value`: `string` — the ID of the preceding command or event.
- `TenantId.Value`: `string` — a tenant-identifying slug (e.g. `"tenant-us-east"`).

---

### Metadata and Builders

#### `EventMetadata`
Immutable record with ambient metadata. `CustomHeaders` is internally backed by `FrozenDictionary<string, string>` but exposed as `IReadOnlyDictionary<string, string>`.
```csharp
namespace EricksonLopez.Events.Metadata;

public sealed record EventMetadata
{
    public CorrelationId CorrelationId { get; init; }   // default: CorrelationId.Empty
    public CausationId CausationId { get; init; }       // default: CausationId.Empty
    public TenantId TenantId { get; init; }             // default: TenantId.Empty
    public string? Source { get; init; }
    public string? ContentType { get; init; }
    public IReadOnlyDictionary<string, string> CustomHeaders { get; init; }  // backed by FrozenDictionary

    public static EventMetadata Empty { get; }
    public static EventMetadata Create(
        CorrelationId? correlationId = null,
        CausationId? causationId = null,
        TenantId? tenantId = null,
        string? source = null);
    public EventMetadata WithHeader(string key, string value);
    public bool TryGetHeader(string key, out string? value);
}
```

#### `EventMetadataBuilder`
Fluent builder for composing `EventMetadata`.
```csharp
namespace EricksonLopez.Events.Metadata;

public sealed class EventMetadataBuilder
{
    public EventMetadataBuilder WithCorrelationId(CorrelationId correlationId);
    public EventMetadataBuilder WithCausationId(CausationId causationId);
    public EventMetadataBuilder WithTenantId(TenantId tenantId);
    public EventMetadataBuilder WithSource(string source);
    public EventMetadataBuilder WithContentType(string contentType);
    public EventMetadataBuilder WithHeader(string key, string value);
    public EventMetadata Build();
}
```

---

### Declarative Attributes

- **`[EventName("semantic.name")]`**: Specifies the stable semantic event name.
- **`[EventVersion(1)]`**: Specifies the integer contract version.
- **`[EventSource("service-name")]`**: Specifies the source URI or identifier.

---

## 2. EricksonLopez.Events

In-process event bus engine, strongly typed envelopes, middleware pipeline, and execution strategies.

### `EventEnvelope<TEvent>`
Immutable, strongly typed **reference type** (`sealed record` class) that combines the event with its ambient metadata. Heap-allocated by design for transport safety (see [ADR-023](adr/adr-023-envelope-record-vs-struct.md)).
```csharp
namespace EricksonLopez.Events.Envelopes;

public sealed record EventEnvelope<TEvent> : IEventEnvelope where TEvent : IEvent
{
    public EventId Id { get; init; }
    public EventType Type { get; init; }
    public EventVersion Version { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public TEvent Payload { get; init; }
    public EventMetadata Metadata { get; init; }

    // Note: GetPayload() is an explicit interface implementation (IEventEnvelope.GetPayload).
    // It is not directly callable as a public method. Use the Payload property instead.
    object IEventEnvelope.GetPayload();
}
```

#### Factory `EventEnvelope`
```csharp
namespace EricksonLopez.Events.Envelopes;

public static class EventEnvelope
{
    // Creates an envelope, resolving EventType and EventVersion from [EventName]/[EventVersion] attributes.
    public static EventEnvelope<TEvent> Create<TEvent>(
        TEvent @event,
        EventMetadata? metadata = null,
        EventType? type = null,
        EventVersion? version = null) where TEvent : IEvent;

    // Convenience overload that builds EventMetadata from individual identity values.
    public static EventEnvelope<TEvent> Wrap<TEvent>(
        TEvent @event,
        CorrelationId? correlationId = null,
        CausationId? causationId = null,
        TenantId? tenantId = null,
        string? source = null) where TEvent : IEvent;
}
```

---

### Bus Configuration (`EventBusServiceCollectionExtensions`)

```csharp
namespace EricksonLopez.Events.Bus.Extensions;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, Action<EventBusOptions>? configure = null);
    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEvent : class, IEvent
        where THandler : class, IEventHandler<TEvent>;
    // Default lifetime is Transient to ensure safe scoped DI resolution within middleware chains.
    public static IServiceCollection AddEventMiddleware<TMiddleware>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMiddleware : class, IEventMiddleware;
}
```

---

### Execution Options and Policies

#### `EventBusOptions`
```csharp
namespace EricksonLopez.Events.Bus.Configuration;

public sealed class EventBusOptions
{
    public EventExecutionMode ExecutionMode { get; set; } = EventExecutionMode.Sequential;
    public ErrorHandlingPolicy ErrorPolicy { get; set; } = ErrorHandlingPolicy.FailFast;
    public bool ThrowOnUnregisteredEvent { get; set; } = false;
    public int MaxReentrancyDepth { get; set; } = 10;
}
```

- **`EventExecutionMode`**:
  - `Sequential` (0): Deterministic serial execution.
  - `Parallel` (1): Concurrent execution via `Task.WhenAll`.
- **`ErrorHandlingPolicy`**:
  - `FailFast` (0): Throws the exception immediately on the first failure.
  - `AggregateAndContinue` (1): Executes all handlers and accumulates failures in `EventDispatchException`.

---

### Pipeline Middlewares

#### `IEventMiddleware`
```csharp
namespace EricksonLopez.Events.Bus.Middleware;

public interface IEventMiddleware
{
    ValueTask InvokeAsync<TEvent>(
        TEvent eventInstance,
        EventMiddlewareDelegate<TEvent> nextHandler,
        CancellationToken cancellationToken) where TEvent : IEvent;
}

public delegate ValueTask EventMiddlewareDelegate<in TEvent>(TEvent eventInstance, CancellationToken cancellationToken) where TEvent : IEvent;
```

---

## 3. EricksonLopez.Events.Serialization.SystemTextJson

Provides high-performance, zero-allocation JSON converters compatible with Native AOT:
- `EventIdJsonConverter`
- `EventTypeJsonConverter`
- `EventVersionJsonConverter`
- `CorrelationIdJsonConverter`
- `CausationIdJsonConverter`
- `TenantIdJsonConverter`
- `EventMetadataJsonConverter`
- `EventEnvelopeJsonConverter<TEvent>`

```csharp
namespace EricksonLopez.Events.Serialization.SystemTextJson;

public static class EventsJsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions AddEventsConverters(this JsonSerializerOptions options);
    public static JsonSerializerOptions CreateDefaultOptions();
}
```

---

## 4. EricksonLopez.Events.Generators

Incremental Roslyn code generator and static code analyzers:
- **`EventIncrementalGenerator`**: Automatically discovers `IEvent` and `IEventHandler<T>` types to emit `GeneratedEventRegistry` and `AddGeneratedEventHandlers` dependency injection methods.
- **Analyzers**:
  - `ELE001`: `IEvent` implementation property immutability — all properties must be `init`-only or read-only.
  - `ELE002`: `[EventName]` attribute must have a non-empty, non-null semantic event name.
  - `ELE003`: `[EventVersion]` attribute must have a positive integer version (>= 1).
  - `ELE004`: `[EventSource]` attribute must have a non-empty, non-null source identifier.
  - `ELE005`: Detection of `IDomainEvent` types incorrectly used as `IIntegrationEvent` (bounded context leakage).

---

## 5. EricksonLopez.Events.CloudEvents

Bidirectional interoperability with the CNCF CloudEvents v1.0 specification.

#### `CloudEvent<TData>`
```csharp
namespace EricksonLopez.Events.CloudEvents;

public sealed record CloudEvent<TData>
{
    public string SpecVersion { get; init; } = "1.0";
    public string Id { get; init; }
    public Uri Source { get; init; }
    public string Type { get; init; }
    public DateTimeOffset? Time { get; init; }
    public string? DataContentType { get; init; }
    public Uri? DataSchema { get; init; }
    public TData? Data { get; init; }
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public string? TenantId { get; init; }
}
```

#### Extension Methods
```csharp
namespace EricksonLopez.Events.CloudEvents;

public static class CloudEventExtensions
{
    // defaultSource: Fallback producer URI when metadata.Source is absent or not a valid URI.
    public static CloudEvent<TEvent> ToCloudEvent<TEvent>(
        this EventEnvelope<TEvent> envelope,
        Uri? defaultSource = null,
        Uri? schemaBaseUri = null) where TEvent : IEvent;

    public static EventEnvelope<TEvent> ToEventEnvelope<TEvent>(
        this CloudEvent<TEvent> cloudEvent) where TEvent : IEvent;
}
```

---

## 6. EricksonLopez.Events.OpenTelemetry

Native integration with OpenTelemetry Tracing and Metrics:
```csharp
namespace EricksonLopez.Events.OpenTelemetry;

public static class EventsOpenTelemetryExtensions
{
    public static TracerProviderBuilder AddEventsInstrumentation(this TracerProviderBuilder builder);
    public static MeterProviderBuilder AddEventsInstrumentation(this MeterProviderBuilder builder);
}
```

---

## 7. EricksonLopez.Events.Testing

Test doubles, spies, and fluent assertion DSL for high-level unit testing:

#### `FakeEventPublisher`
```csharp
namespace EricksonLopez.Events.Testing;

public sealed class FakeEventPublisher : IEventPublisher
{
    public int Count { get; }
    public IReadOnlyList<IEvent> PublishedEvents { get; }

    // Fluent assertion methods (throw InvalidOperationException on failure)
    public FakeEventPublisher ShouldHavePublished<TEvent>() where TEvent : IEvent;
    public FakeEventPublisher ShouldHavePublished<TEvent>(Func<TEvent, bool> predicate) where TEvent : IEvent;
    public FakeEventPublisher ShouldHavePublishedCount<TEvent>(int expectedCount) where TEvent : IEvent;
    public FakeEventPublisher ShouldNotHavePublished<TEvent>() where TEvent : IEvent;
    public FakeEventPublisher ShouldNotHavePublished<TEvent>(Func<TEvent, bool> predicate) where TEvent : IEvent;

    // Retrieval helpers
    public IReadOnlyList<TEvent> GetEvents<TEvent>() where TEvent : IEvent;
    public IReadOnlyList<TEvent> GetEvents<TEvent>(Func<TEvent, bool> predicate) where TEvent : IEvent;
    public TEvent GetSingleEvent<TEvent>() where TEvent : IEvent;

    // failureCount: Number of subsequent PublishAsync calls that should throw exception (default: 1).
    public void SimulateFailure(Exception exception, int failureCount = 1);

    // Clears all published events and resets simulated failure state.
    public void Reset();
}
```

#### `TestEventHandler<TEvent>`
```csharp
namespace EricksonLopez.Events.Testing;

public sealed class TestEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
{
    public bool WasInvoked { get; }                               // true if invoked at least once
    public int InvocationCount { get; }                          // total number of HandleAsync calls
    public IReadOnlyList<TEvent> HandledEvents { get; }          // all handled events, chronological
    public IReadOnlyList<DateTimeOffset> ExecutionTimestamps { get; } // UTC timestamps of each invocation
    public TEvent? LastEvent { get; }                            // last event handled, or null

    // Fluent configuration (chainable)
    public TestEventHandler<TEvent> WithDelay(TimeSpan delay);   // simulates async delay
    public TestEventHandler<TEvent> WithException(Exception exception); // throws on every invocation
    public TestEventHandler<TEvent> WithCallback(Action<TEvent> callback); // sync callback
    public TestEventHandler<TEvent> WithCallback(Func<TEvent, CancellationToken, ValueTask> asyncCallback); // async callback

    // Clears all records and resets delay/exception/callback configuration.
    public void Reset();
}
```

#### `EventTestBuilder`
```csharp
namespace EricksonLopez.Events.Testing;

public static class EventTestBuilder
{
    // Entry point for the fluent builder.
    public static EventEnvelopeTestBuilder<TEvent> For<TEvent>(TEvent payload) where TEvent : IEvent;
}
```

#### `EventEnvelopeTestBuilder<TEvent>`
```csharp
namespace EricksonLopez.Events.Testing;

// Fluent builder for constructing synthetic EventEnvelope<TEvent> instances in unit tests.
public sealed class EventEnvelopeTestBuilder<TEvent> where TEvent : IEvent
{
    public EventEnvelopeTestBuilder(TEvent payload);

    // Identity and temporal overrides
    public EventEnvelopeTestBuilder<TEvent> WithId(EventId id);
    public EventEnvelopeTestBuilder<TEvent> WithType(string eventType);
    public EventEnvelopeTestBuilder<TEvent> WithVersion(uint version);
    public EventEnvelopeTestBuilder<TEvent> WithOccurredAt(DateTimeOffset occurredAt);

    // Metadata overrides
    public EventEnvelopeTestBuilder<TEvent> WithCorrelationId(string correlationId);
    public EventEnvelopeTestBuilder<TEvent> WithCausationId(string causationId);
    public EventEnvelopeTestBuilder<TEvent> WithTenantId(string tenantId);
    public EventEnvelopeTestBuilder<TEvent> WithSource(string source);
    public EventEnvelopeTestBuilder<TEvent> WithContentType(string contentType);
    public EventEnvelopeTestBuilder<TEvent> WithHeader(string key, string value);

    // Builds the configured EventEnvelope<TEvent>.
    public EventEnvelope<TEvent> Build();
}
```

**Usage example:**
```csharp
var envelope = EventTestBuilder
    .For(new OrderPlacedIntegrationEvent(EventId.New(), Guid.NewGuid(), 99.99m, "USD", DateTimeOffset.UtcNow))
    .WithCorrelationId("test-correlation-123")
    .WithTenantId("tenant-a")
    .WithHeader("X-Test-Environment", "CI")
    .Build();
```


