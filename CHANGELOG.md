# Changelog

All notable changes to `EricksonLopez.Events` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-08-24

### Added

#### Core Contracts & Identifiers (`EricksonLopez.Events.Contracts`)
- **`IEvent`**: Foundational event contract defining monotonically ordered `EventId Id` and `DateTimeOffset OccurredAt`.
- **`IDomainEvent`**: Marker contract for in-process facts raised strictly within Aggregate Roots and Domain Boundaries.
- **`IIntegrationEvent`**: Marker contract for versioned, public cross-boundary integration events published to outboxes and message brokers.
- **`EventId`**: 16-byte zero-allocation `readonly record struct` powered by native GUID Version 7 (`Guid.CreateVersion7()`) with `ISpanFormattable`, `IUtf8SpanFormattable`, and `IParsable<EventId>`.
- **`EventType`**: Strongly-typed string-backed value type (`readonly record struct`) for explicit, semantic event type names decoupled from CLR namespaces.
- **`EventVersion`**: Strongly-typed monotonic version identifier (`readonly record struct`) supporting semantic contract evolution.
- **`CorrelationId`, `CausationId`, `TenantId`**: Strongly typed domain contextual identifiers (`readonly record struct`) for ambient correlation, root causation tracing, and multi-tenant partition routing.
- **`EventMetadata` & `EventMetadataBuilder`**: Immutable metadata model backed by `FrozenDictionary<string, string>` for low-allocation, reflection-free header propagation.
- **`EventEnvelope<TEvent>` & `IEventEnvelope`**: Reference-type (`sealed record`) transport envelopes encapsulating payload, typed metadata, and schema versioning.
- **`IEventHandler<in TEvent>`**: Asynchronous event handler abstraction returning `ValueTask`.
- **`IEventPublisher`, `IEventSubscriber`, `IEventBus`**: Clean publication and subscription abstractions.

#### Core Event Bus & Dispatching (`EricksonLopez.Events`)
- **`EventBus`**: High-throughput, thread-safe, reflection-free in-memory event orchestrator with middleware pipeline support, reentrancy guards, and cancellation propagation.
- **`IExecutionStrategy`**: Configurable execution strategies including `SequentialExecutionStrategy` and `ParallelExecutionStrategy`.
- **`ErrorHandlingPolicy`**: Granular error handling policies supporting `FailFast` (immediate abort and throw) and `AggregateAndContinue` (aggregate multi-handler exceptions into `EventDispatchException`).
- **`StaticEventTypeRegistry` & `EventTypeRegistry`**: Static generic cache and O(1) `FrozenDictionary` bidirectional lookup registry with explicit AOT trimming annotations.
- **`InMemoryEventPublisher`**: Lightweight in-memory event publisher utility for testing and local development.
- **`EventsDiagnostics` & `EventBusDiagnostics`**: Zero-cost distributed tracing and metrics powered by standard .NET `ActivitySource` ("EricksonLopez.Events") and `Meter` ("EricksonLopez.Events").

#### Roslyn Source Generator & Analyzers (`EricksonLopez.Events.Generators`)
- **`EventIncrementalGenerator`**: Roslyn incremental source generator generating compile-time `GeneratedEventRegistry` for 100% Native AOT and zero-reflection event discovery.
- **Analyzers**:
  - `ELE001`: Enforces immutability on `IEvent` types — all properties must be `init`-only or read-only.
  - `ELE002`: Validates `[EventVersion]` attribute values — version must be a positive integer (>= 1).
  - `ELE003`: Validates `[EventName]` attribute non-emptiness — semantic event name must be non-null and non-empty.
  - `ELE004`: Validates `[EventSource]` attribute non-emptiness — source identifier must be non-null and non-empty.
  - `ELE005`: Prevents accidental leakage of `IDomainEvent` outside internal domain boundaries.

#### System.Text.Json Native AOT Serialization (`EricksonLopez.Events.Serialization.SystemTextJson`)
- **`EventsJsonSerializerOptionsExtensions`**: Extension method `.AddEventsConverters()` registering zero-reflection JSON converters.
- **Custom JSON Converters**: Native AOT converters for `EventId`, `EventType`, `EventVersion`, `CorrelationId`, `CausationId`, `TenantId`, `EventMetadata`, and polymorphic `EventEnvelope<TEvent>`.

#### CloudEvents 1.0 Adapter (`EricksonLopez.Events.CloudEvents`)
- **`CloudEvent`**: CNCF CloudEvents v1.0 standard envelope model.
- **Bidirectional Mapping Extensions**: `.ToCloudEvent()` and `.ToEventEnvelope()` with seamless mapping of `id`, `source`, `type`, `time`, `data`, and custom extension attributes (`correlationid`, `causationid`, `tenantid`).

#### OpenTelemetry Observability Bridge (`EricksonLopez.Events.OpenTelemetry`)
- **`EventsOpenTelemetryExtensions`**: OpenTelemetry tracer and meter registration helpers for `OpenTelemetry.Trace.TracerProviderBuilder` and `OpenTelemetry.Metrics.MeterProviderBuilder`.

#### Idempotent Inbox Consumer (`EricksonLopez.Events.Inbox`)
- **`IdempotentEventHandler<TEvent>`**: Decorator for `IEventHandler<TEvent>` integrating with `EricksonLopez.Inbox.Abstractions` to guarantee exactly-once consumer execution.

#### Transactional Outbox Bridge (`EricksonLopez.Events.Outbox`)
- **`OutboxEventPublisher`**: Bridge implementing `IEventPublisher` that wraps domain events into `EventEnvelope<T>` and stores them in transactional outbox persistence via `EricksonLopez.Outbox.Abstractions`.

#### Public Testing Suite (`EricksonLopez.Events.Testing`)
- **`FakeEventPublisher`**: Thread-safe in-memory recording event publisher for unit testing with failure simulation and fluent assertions.
- **`TestEventHandler<TEvent>`**: In-memory test handler tracking received events, invocation counts, delays, and callbacks.
- **`EventTestBuilder`**: Fluent builder for synthesizing test event instances, metadata, and envelopes.

#### Architecture, Testing & Benchmarking Infrastructure
- **356 Automated Tests**: Solution-wide automated test suite across 10 test projects with 100% code coverage.
- **100% Mutation Testing Score**: Stryker.NET mutation coverage with 9 modular configurations and a 95% break threshold.
- **Continuous Native AOT CI Matrix**: Executable smoke tests compiled with `PublishAot=true` verified on Windows and Ubuntu runners in GitHub Actions.
- **BenchmarkDotNet Suite**: Micro-benchmarks measuring nanosecond-level throughput and zero allocations for identifiers, formatting, envelopes, registries, and dispatch.
