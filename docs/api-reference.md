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
- [7. EricksonLopez.Events.Inbox](#7-ericksonlopezeventsinbox)
- [8. EricksonLopez.Events.Outbox](#8-ericksonlopezeventsoutbox)
- [9. EricksonLopez.Events.Testing](#9-ericksonlopezeventstesting)

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

## 7. EricksonLopez.Events.Inbox

Guarantees idempotent consumption and event deduplication:
```csharp
namespace EricksonLopez.Events.Inbox;

public sealed class IdempotentEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
{
    // consumerName: Optional logical consumer group name used for deduplication scoping.
    //               Defaults to the full CLR type name of innerHandler.
    // logger: Optional structured logger; uses NullLogger if not provided.
    public IdempotentEventHandler(
        IEventHandler<TEvent> innerHandler,
        IInboxConsumerFilter inboxFilter,
        string? consumerName = null,
        ILogger<IdempotentEventHandler<TEvent>>? logger = null);
    public ValueTask HandleAsync(TEvent eventInstance, CancellationToken cancellationToken = default);
}

public static class EventInboxServiceCollectionExtensions
{
    // consumerName: Optional consumer group name for deduplication scoping.
    //               Defaults to the full type name of THandler.
    public static IServiceCollection AddIdempotentEventHandler<TEvent, THandler>(
        this IServiceCollection services,
        string? consumerName = null)
        where TEvent : class, IEvent
        where THandler : class, IEventHandler<TEvent>;
}
```

---

## 8. EricksonLopez.Events.Outbox

Atomic transactional publishing via the Transactional Outbox pattern:
```csharp
namespace EricksonLopez.Events.Outbox;

public sealed class OutboxEventPublisher : IEventPublisher
{
    // transactionProvider: Optional transaction coordinator. Defaults to NullOutboxTransactionProvider
    //                      (no-op) if not provided.
    public OutboxEventPublisher(
        IOutbox outbox,
        IOutboxTransactionProvider? transactionProvider = null);
    public ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default) where TEvent : IEvent;
}

/// <summary>Optional interface for ambient transaction coordination in the Outbox pattern.</summary>
public interface IOutboxTransactionProvider
{
    IDbTransaction? CurrentTransaction { get; }
}

public static class EventsOutboxServiceCollectionExtensions
{
    // Registers OutboxEventPublisher with NullOutboxTransactionProvider (Singleton).
    public static IServiceCollection AddOutboxEventPublisher(this IServiceCollection services);
    // Registers OutboxEventPublisher with a custom IOutboxTransactionProvider (Scoped).
    public static IServiceCollection AddOutboxEventPublisher<TTransactionProvider>(this IServiceCollection services)
        where TTransactionProvider : class, IOutboxTransactionProvider;
}
```

---

## 9. EricksonLopez.Events.Testing

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


