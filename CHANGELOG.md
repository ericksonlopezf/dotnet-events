# Changelog

All notable changes to `EricksonLopez.Events` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - 2026-09-20

### Breaking Changes
- **BC-001 (Compile-Time / Contract): `IEventTypeRegistry` Added Overload Without DIM**: Added `bool TryGetDescriptor(EventType eventType, EventVersion version, [NotNullWhen(true)] out EventTypeDescriptor? descriptor)` to public interface `IEventTypeRegistry` without a default interface implementation.
  - *Previous State:* Only single-argument overloads `(EventType, out EventTypeDescriptor?)` and `(Type, out EventTypeDescriptor?)` existed.
  - *Current State:* New binary overload `(EventType, EventVersion, out EventTypeDescriptor?)` is declared abstractly on the interface.
  - *Affected Consumers:* External classes implementing `IEventTypeRegistry`.
  - *Migration:* Implement the new `TryGetDescriptor(EventType, EventVersion, out EventTypeDescriptor?)` overload in custom registry implementations.

- **BC-002 (Runtime / Binary Compatibility): `HandlerDescriptor` Constructor Signature Changed**: Replaced the 3-parameter constructor `HandlerDescriptor(Type, Type, Func<object, object, CancellationToken, ValueTask>)` with a 4-parameter constructor `HandlerDescriptor(Type, Type, Func<object, object, CancellationToken, ValueTask>, object? typedInvoker = null)`.
  - *Previous State:* Public constructor had 3 arguments.
  - *Current State:* Single constructor with 4 arguments (optional 4th). The 3-parameter `.ctor` method token was removed from assembly metadata.
  - *Affected Consumers:* Assemblies compiled against v1.0.0 directly invoking `new HandlerDescriptor(handlerType, serviceType, invoker)` without recompiling.
  - *Migration:* Recompile consumer projects against v2.0.0, or pass `typedInvoker: null` explicitly.

- **BC-003 (Configuration / Runtime): `AddEventMiddleware` Enforces Immutability on Singleton Middlewares**: Registering an `IEventMiddleware` with `ServiceLifetime.Singleton` via `services.AddEventMiddleware<TMiddleware>(ServiceLifetime.Singleton)` now validates that `TMiddleware` contains no mutable instance fields.
  - *Previous State:* Any middleware could be registered as `Singleton` regardless of instance field mutability.
  - *Current State:* Throws `InvalidOperationException` at DI registration time if the middleware contains non-readonly instance fields that are not `AsyncLocal<T>`.
  - *Affected Consumers:* Applications registering stateful middlewares as singletons.
  - *Migration:* Change the middleware registration lifetime to `ServiceLifetime.Transient` (default) or `ServiceLifetime.Scoped`, make instance fields `readonly`, or encapsulate per-request state in `AsyncLocal<T>`.

- **BC-004 (Runtime / Behavioral): `StaticEventTypeRegistry.Current` Setter Disallows Re-Initialization and Null**: Setting `StaticEventTypeRegistry.Current` after initial setup now throws `InvalidOperationException`. Setting `Current = null` now throws `ArgumentNullException`.
  - *Previous State:* `Current` setter allowed multiple re-assignments, and assigning `null` reset the registry to `EventTypeRegistry.Empty`.
  - *Current State:* Setter enforces write-once semantics to prevent in-process registry poisoning. Re-assigning throws `InvalidOperationException`. Setting `null` throws `ArgumentNullException`.
  - *Affected Consumers:* Test fixtures or modular applications re-initializing `StaticEventTypeRegistry.Current` across tests or modules.
  - *Migration:* Use `StaticEventTypeRegistry.SetCurrent(registry, allowOverride: true)` for test isolation, or call `StaticEventTypeRegistry.Reset()` between test runs.

- **BC-005 (Runtime / Exception): `CloudEventExtensions.ToCloudEvent` Throws on Normalized Key Collision**: When converting `EventMetadata.CustomHeaders` to CloudEvent extension attributes, duplicate keys that normalize to the same lowercase alphanumeric identifier with differing values now throw `InvalidOperationException`.
  - *Previous State:* Later duplicate normalized keys silently overwrote earlier entries in `extensionAttributes`.
  - *Current State:* Throws `InvalidOperationException` with details identifying the conflicting keys and values.
  - *Affected Consumers:* Envelopes carrying custom headers that differ only in casing or hyphens (e.g. `X-Tenant-Id` and `xtenantid`) with distinct values.
  - *Migration:* Ensure custom header keys normalize uniquely under lowercase hyphen-stripped naming conventions before calling `.ToCloudEvent()`.

- **BC-006 (Behavioral): `CloudEventExtensions.ToEventEnvelope` Schema Version Resolution Changed**: Converting a `CloudEvent<TEvent>` without a `DataSchema` URI matching `/v{N}` now defaults `Version` directly to `EventVersion.V1` (1) instead of resolving from `StaticEventTypeRegistry.GetVersion<TEvent>()`.
  - *Previous State:* Passed `version: default`, causing the `EventEnvelope` constructor to look up `StaticEventTypeRegistry.GetVersion<TEvent>()` (e.g., version 2 or higher if configured).
  - *Current State:* Version defaults strictly to `EventVersion.V1` (1) unless explicitly parsed from `DataSchema`.
  - *Affected Consumers:* CloudEvent consumers ingesting schema-less CloudEvents for versioned event contracts (v2+).
  - *Migration:* Populate `cloudEvent.DataSchema` with a schema URI containing `/v{version}` (e.g. `https://schema.example.com/events/OrderPlaced/v2`) or construct the `EventEnvelope` explicitly.

- **BC-007 (Behavioral / Equality): `EventVersion` Default Struct Semantics and Value Changed**: `default(EventVersion).Value` is now `1` instead of `0`. `default(EventVersion) == EventVersion.V1` now returns `true`.
  - *Previous State:* Auto-property `Value` evaluated to `0` for uninitialized `default(EventVersion)`. `default(EventVersion) == EventVersion.V1` returned `false`.
  - *Current State:* Property getter evaluates `_value == 0 ? 1 : _value`. `default(EventVersion) == EventVersion.V1` evaluates to `true`. Added `IsUninitialized` property to check for default state.
  - *Affected Consumers:* Code checking `version.Value == 0` to detect uninitialized struct state or expecting `default(EventVersion) != EventVersion.V1`.
  - *Migration:* Replace `version.Value == 0` checks with `version.IsUninitialized`.

- **BC-008 (Behavioral / Equality): `TenantId` Case-Insensitivity and Whitespace Equality**: `TenantId.IsEmpty` now returns `true` for whitespace strings (`string.IsNullOrWhiteSpace`), and equality (`Equals`, `==`, `GetHashCode`, `CompareTo`) uses `StringComparison.OrdinalIgnoreCase`.
  - *Previous State:* `IsEmpty` checked `string.IsNullOrEmpty` (whitespace was considered non-empty). Equality was case-sensitive ordinal comparison (`"tenant-a" != "TENANT-A"`).
  - *Current State:* Whitespace is empty. Equality treats `"tenant-a" == "TENANT-A"` as `true`. Two empty/whitespace instances compare equal.
  - *Affected Consumers:* Consumers relying on case-sensitive tenant identifiers or storing distinct tenants differing only by case.
  - *Migration:* Audit tenant routing logic. Ensure tenant identifiers do not depend on case distinctions.

- **BC-009 (Behavioral / Equality): `EventType` Case-Insensitive Equality**: `EventType.Equals` and `GetHashCode` now use `StringComparison.OrdinalIgnoreCase`.
  - *Previous State:* Case-sensitive ordinal comparison (`new EventType("OrderCreated") != new EventType("ordercreated")`).
  - *Current State:* Case-insensitive comparison (`new EventType("OrderCreated") == new EventType("ordercreated")`).
  - *Affected Consumers:* Code distinguishing event types solely by casing in sets, dictionaries, or equality checks.
  - *Migration:* Ensure unique semantic event names are used rather than case-only variations.

- **BC-010 (Behavioral / Exception): `ParallelExecutionStrategy` FailFast Error Policy Rethrows Inner Exception**: When executing handlers in parallel under `ErrorHandlingPolicy.FailFast`, handler exceptions are now rethrown directly via `ExceptionDispatchInfo` instead of being wrapped in `EventDispatchException`.
  - *Previous State:* Multiple handler errors were captured and thrown as `EventDispatchException(typeof(TEvent), exceptions)`.
  - *Current State:* The first handler exception is rethrown directly.
  - *Affected Consumers:* Callers catching `EventDispatchException` when `EventBusOptions.ErrorPolicy = ErrorHandlingPolicy.FailFast` in parallel mode.
  - *Migration:* Catch specific exception types directly (e.g. `InvalidOperationException`), or configure `ErrorHandlingPolicy.AggregateAndContinue` if multi-exception aggregation in `EventDispatchException` is desired.

- **BC-011 (Behavioral / Lifecycle): `ParallelExecutionStrategy` Default Handler Scope Policy Changed to `Auto`**: In parallel execution mode, `EventBusOptions.ScopePolicy` defaults to `HandlerScopePolicy.Auto`, creating an isolated `IServiceScope` per handler when `IServiceScopeFactory` is available.
  - *Previous State:* All parallel handlers resolved services from the ambient `IServiceProvider` scope, sharing scoped dependencies across concurrent threads.
  - *Current State:* Each parallel handler executes within an isolated `IServiceScope`, ensuring thread safety for scoped services (e.g. `DbContext`).
  - *Affected Consumers:* Systems expecting parallel handlers to share ambient scoped service instances within a publish call.
  - *Migration:* If sharing the ambient scope is explicitly required and thread-safe, set `options.ScopePolicy = HandlerScopePolicy.ReuseAmbientScope` (only allowed in sequential mode or with thread-safe dependencies).

- **BC-012 (Runtime / Behavioral): `InMemoryEventPublisher` Reentrancy Depth Limit**: Added `MaxReentrancyDepth` (default `10`) to `InMemoryEventPublisher`, throwing `InvalidOperationException` when cascading handler publications exceed the limit.
  - *Previous State:* Publications were unbounded, allowing deep or infinite recursive publishing until stack overflow occurred.
  - *Current State:* Throws `InvalidOperationException: "InMemoryEventPublisher maximum reentrancy depth limit (10) exceeded..."`.
  - *Affected Consumers:* Deep cascading event publication flows running against `InMemoryEventPublisher` in test or development environments.
  - *Migration:* Increase `inMemoryPublisher.MaxReentrancyDepth = desiredDepth` or refactor cascading publication chains into discrete workflows.

- **BC-013 (Compile-Time / Analyzer): Roslyn Analyzer `ELE001` Flags Public Mutable Fields on Event Types**: Analyzer `ELE001` (Severity: `Error`) now validates public fields in addition to properties on types implementing `IEvent`.
  - *Previous State:* Checked only properties with mutable setters. Public fields were ignored.
  - *Current State:* Public non-readonly, non-const fields on event types trigger compiler error `ELE001`.
  - *Affected Consumers:* Event types declared with public mutable fields (e.g. `public int Retries;`).
  - *Migration:* Change fields to `readonly` or convert to `init`-only record properties (e.g. `public int Retries { get; init; }`).

- **BC-014 (Runtime / Contract): `IEventSubscriber` Throws `NotSupportedException` for Envelope Handlers on Legacy Implementations**: Added `Subscribe<TEvent>(IEnvelopeEventHandler<TEvent>)` and `Unsubscribe<TEvent>(IEnvelopeEventHandler<TEvent>)` to `IEventSubscriber` with default implementations throwing `NotSupportedException`.
  - *Previous State:* `IEventSubscriber` only supported `IEventHandler<TEvent>`.
  - *Current State:* Default interface implementations throw `NotSupportedException`.
  - *Affected Consumers:* Custom implementations of `IEventSubscriber` from v1.0.0 when consumed by code attempting to register envelope handlers.
  - *Migration:* Override `Subscribe<TEvent>(IEnvelopeEventHandler<TEvent>)` and `Unsubscribe<TEvent>(IEnvelopeEventHandler<TEvent>)` in custom `IEventSubscriber` implementations.

- **BC-015 (Compile-Time / Analyzer): Roslyn Analyzer `ELE006` Flags Mutable Collections on Event Types**: Added diagnostic analyzer rule `ELE006` (Severity: `Warning`) detecting mutable collection types (such as `List<T>`, `Dictionary<TKey, TValue>`, `T[]`, `HashSet<T>`, etc.) declared on event contract properties.
  - *Previous State:* Analyzer `ELE006` did not exist. Event types with mutable collection properties compiled without diagnostic warnings.
  - *Current State:* Public properties returning mutable collections trigger analyzer warning `ELE006`. In consumer projects with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, this warning results in a build compilation error.
  - *Affected Consumers:* Consumers declaring event contracts with mutable collection properties (e.g., `public List<OrderItem> Items { get; init; }`).
  - *Migration:* Replace mutable collection types with immutable collections (e.g., `IReadOnlyList<T>`, `ImmutableArray<T>`, `FrozenSet<T>`).

- **BC-016 (Runtime / Serialization): `EventEnvelopeJsonConverter<TEvent>` Strictly Enforces `payload` Property Presence on Struct Payloads**: Deserializing an `EventEnvelope<TEvent>` when `TEvent` is a value type (`record struct` / `struct`) now throws `JsonException` if the JSON payload property is omitted.
  - *Previous State:* The check `payload is null` evaluated to `false` for value types, causing JSON envelopes missing the `"payload"` property to silently deserialize with an uninitialized `default(TEvent)` payload.
  - *Current State:* The converter tracks whether `"payload"` was encountered during JSON reading (`hasPayload`). If omitted, it throws `JsonException: "Missing payload property when deserializing EventEnvelope<TEvent>."`.
  - *Affected Consumers:* Systems deserializing partial or malformed envelope JSON payloads wrapping struct event types.
  - *Migration:* Ensure envelope JSON payloads explicitly contain the `payload` property, or handle `JsonException` during deserialization.

- **BC-017 (Behavioral): `EventEnvelope<TEvent>` Constructor Automatically Defaults `Metadata.Source` from `[EventSource]`**: Constructing an `EventEnvelope<TEvent>` where `metadata.Source` is null or empty now populates `Metadata.Source` with `descriptor.Source` from `StaticEventTypeRegistry.GetDescriptor<TEvent>()`.
  - *Previous State:* If `metadata.Source` was null or omitted, `envelope.Metadata.Source` remained `null`.
  - *Current State:* If `metadata.Source` is null or empty, it defaults to the `[EventSource]` attribute value registered on the event type descriptor.
  - *Affected Consumers:* Code expecting `envelope.Metadata.Source` to remain `null` when omitted, or code comparing metadata instances where source defaulting alters equality.
  - *Migration:* If `Source` must be omitted or explicitly empty, pass a metadata instance with an explicit non-null override or accommodate the populated source in downstream validation.

- **BC-018 (Behavioral / Equality): `EventMetadata` Implements Deep Value Equality on `CustomHeaders`**: `EventMetadata.Equals` and `GetHashCode` now evaluate case-insensitive dictionary contents rather than reference equality of the underlying `FrozenDictionary`.
  - *Previous State:* As an auto-generated record, `CustomHeaders` was compared using default reference equality. Two `EventMetadata` instances with identical headers evaluated to `false` via `==` and `Equals`.
  - *Current State:* Implements deep value equality across all headers using `OrdinalIgnoreCase` for keys and `Ordinal` for values. Two instances with matching headers compare `true`.
  - *Affected Consumers:* Code expecting distinct `EventMetadata` instances with identical headers to maintain distinct identity in sets or dictionaries.
  - *Migration:* Review test assertions and set/map key usages where `EventMetadata` reference inequality was relied upon.

### Added
- **`EventContext`**: Ambient async-local context (`EventContext.Current`, `Metadata`, `TenantId`, `CorrelationId`, `CausationId`, `ExecutionTracker`, `SetCurrent`, `SetExecutionTracker`) enabling transparent metadata and execution tracking across asynchronous dispatch pipelines.
- **`IEnvelopeEventHandler<in TEvent>`**: Special-purpose handler contract in `EricksonLopez.Events.Contracts` allowing direct access to `IEventEnvelope<TEvent>` and ambient metadata.
- **`IEventEnvelope<out TEvent>`**: Covariant generic envelope contract enabling polymorphic envelope processing without casting.
- **`HandlerScopePolicy`**: Configurable DI scope resolution strategy (`Auto`, `CreatePerHandler`, `ReuseAmbientScope`) within `EventBusOptions` for parallel dispatch.
- **`CausationDepthLimitMiddleware`**: Built-in middleware protecting in-process event pipelines from cyclic and runaway cascading event emission.
- **`EventEnvelopeJsonConverterFactory`**: Polymorphic `System.Text.Json` converter factory for Native AOT-safe serialization of arbitrary `EventEnvelope<TEvent>` types.
- **Roslyn Analyzer `ELE006`**: Diagnostic analyzer enforcing deep immutability by warning against mutable collection types on `IEvent` properties.
- **`GeneratedEventServiceCollectionExtensions`**: Compile-time generated `services.AddGeneratedEventHandlers()` for zero-reflection DI registration with strongly typed invokers.
- **Benchmark Regression Gate**: Automated CI gate (`verify-benchmark-gate.ps1`, `benchmark-regression-gate.yml`) enforcing a strict 0 B hot-path heap allocation invariant and 5% latency regression threshold vs baseline.

### Changed
- **AOT Smoke Test Project**: Replaced legacy `NativeAotTests` test harness with dedicated executable project `EricksonLopez.Events.AotSmokeTest` configured for standalone compilation with `PublishAot=true`.
- **Stryker Configuration Standardization**: Modularized Stryker mutation configurations to canonical `stryker-*.json` naming convention synchronized with matrix execution in `mutation-testing.yml`.

### Removed
- **In-Memory Transactional Publisher**: Removed `ITransactionalEventPublisher` and `TransactionalEventPublisher` from core in v2.0.0 due to in-memory dual-write data loss risks upon process crash; all durable transactional staging is standardized on `EricksonLopez.Outbox` ([ADR-037](docs/adr/adr-037-deprecation-of-in-memory-transactional-publisher.md)).

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

#### Public Testing Suite (`EricksonLopez.Events.Testing`)
- **`FakeEventPublisher`**: Thread-safe in-memory recording event publisher for unit testing with failure simulation and fluent assertions.
- **`TestEventHandler<TEvent>`**: In-memory test handler tracking received events, invocation counts, delays, and callbacks.
- **`EventTestBuilder`**: Fluent builder for synthesizing test event instances, metadata, and envelopes.

#### Architecture, Testing & Benchmarking Infrastructure
- **327 Automated Tests**: Solution-wide automated test suite across 8 test projects with 100% code coverage.
- **100% Mutation Testing Score**: Stryker.NET mutation coverage with 7 modular configurations and a 95% break threshold.
- **Continuous Native AOT CI Matrix**: Executable smoke tests compiled with `PublishAot=true` verified on Windows and Ubuntu runners in GitHub Actions.
- **BenchmarkDotNet Suite**: Micro-benchmarks measuring nanosecond-level throughput and zero allocations for identifiers, formatting, envelopes, registries, and dispatch.

[Unreleased]: https://github.com/ericksonlopezf/dotnet-events/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/ericksonlopezf/dotnet-events/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/ericksonlopezf/dotnet-events/releases/tag/v1.0.0
