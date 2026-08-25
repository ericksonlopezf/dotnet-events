# EricksonLopez.Events — Feature Matrix

> **Architectural decision document. Not a wish list. Every decision recorded here has been derived from code analysis, ecosystem boundaries, and ADRs.**
>
> Last updated: 2026-08-24

---

## Table of Contents

1. [Vision](#1-vision)
2. [Scope & Bounded Responsibility](#2-scope--bounded-responsibility)
3. [Architectural Boundaries](#3-architectural-boundaries)
4. [Core Feature Matrix](#4-core-feature-matrix)
5. [Optional Features](#5-optional-features)
6. [Ecosystem Features](#6-ecosystem-features)
7. [Rejected Features](#7-rejected-features)
8. [Future Features](#8-future-features)
9. [Competitive Analysis](#9-competitive-analysis)
10. [API Surface](#10-api-surface)
11. [Performance Benchmarks](#11-performance-benchmarks)
12. [AOT Guarantee Matrix](#12-aot-guarantee-matrix)
13. [Roadmap](#13-roadmap)
14. [ADR Index](#14-adr-index)
15. [Package Compatibility Policy](#15-package-compatibility-policy)
16. [Testing Strategy](#16-testing-strategy)

---

## 1. Vision

`EricksonLopez.Events` is **the single authoritative source of truth for event modeling, contracts, and identity** within the EricksonLopez ecosystem.

It provides the immutable types, contracts, metadata, and descriptors that every other component in the ecosystem consumes — without knowing about transport, persistence, broker, serialization format, or mediation pipeline.

> The goal is NOT the library with the most features.
> The goal is the library with the clearest architectural responsibility, smallest public API, best performance, and deepest integration with DDD/Clean Architecture/AOT-first .NET.

---

## 2. Scope & Bounded Responsibility

### What EricksonLopez.Events IS

| Included | Description |
|---|---|
| **Pure Event Contracts** | `IEvent`, `IDomainEvent`, `IIntegrationEvent` |
| **Monotonic Identity** | `EventId` (Guid v7), `EventType`, `EventVersion` |
| **Typed Ambient Metadata** | `CorrelationId`, `CausationId`, `TenantId`, `EventMetadata` |
| **Typed Event Envelope** | `EventEnvelope<TEvent>`, `IEventEnvelope` |
| **Static Event Registry** | `IEventTypeRegistry`, `EventTypeDescriptor`, `StaticEventTypeRegistry` |
| **Minimal Handler Contract** | `IEventHandler<in TEvent>` with `ValueTask` |
| **Minimal Publisher Contract** | `IEventPublisher`, `IEventSubscriber` |
| **In-Process Utility Publisher** | `InMemoryEventPublisher` (utility, not production) |
| **BCL Observability** | `EventsDiagnostics` via `ActivitySource` + `Meter` |
| **Zero-Reflection Source Generator** | `EricksonLopez.Events.Generators` |
| **AOT-Safe JSON Serialization** | `EricksonLopez.Events.Serialization.SystemTextJson` |

### What EricksonLopez.Events IS NOT

| Excluded | Belongs to |
|---|---|
| Message broker client (Kafka, RabbitMQ, Azure Service Bus) | Transport adapters |
| Transactional Outbox persistence | `EricksonLopez.Outbox` |
| Mediator pipeline behaviors | `EricksonLopez.Mediator` |
| Distributed saga orchestration | Workflow engine |
| Retry / circuit breaker policies | Resilience libraries |
| Assembly scanning | Replaced by Source Generator |
| Dynamic proxy / IL emission | Incompatible with AOT |
| Schema registry | External infrastructure |
| Event sourcing / event store | Different architectural paradigm |

---

## 3. Architectural Boundaries

### 3.1 Ecosystem Dependency Diagram

```
EricksonLopez.Events (0 external deps, BCL-only)
         |
         |---> EricksonLopez.SharedKernel (may depend on Events)
         |
         |---> EricksonLopez.Mediator (implements IEventPublisher, IEventHandler)
         |
         |---> EricksonLopez.Outbox (persists EventEnvelope)
         |         `---> EricksonLopez.Events.Serialization.SystemTextJson
         |
         `---> Infrastructure / Transport adapters
```

### 3.2 Package Dependency Matrix

| Package | Can depend on Events | Can publish | Can persist | Can know Broker |
|---|:---:|:---:|:---:|:---:|
| Domain | Yes | No direct | No | No |
| SharedKernel | Yes | No | No | No |
| Application | Yes | Yes via IEventPublisher | No | No |
| Events | Self (BCL only) | Yes in-process | No | No |
| Mediator | Yes | Yes dispatch | No | No |
| Outbox | Yes | Yes persist+republish | Yes | No |
| Infrastructure | Yes | Yes | Yes | Yes |
| API Layer | Yes | Yes via mediator | No | No |

### 3.3 Full Event Lifecycle

```
Domain Aggregate
  | raises IDomainEvent
  v
Application Handler
  | maps to IIntegrationEvent
  | wraps in EventEnvelope<TEvent> with EventMetadata
  v
EricksonLopez.Outbox
  | persists serialized EventEnvelope in same DB transaction
  v
Outbox Background Worker
  | reads EventEnvelope from DB
  | publishes to Transport adapter
  v
Transport Adapter (Kafka, RabbitMQ, Azure Service Bus)
  | maps EventMetadata to wire headers
  | publishes serialized payload
  v
Consumer Service
  | deserializes EventEnvelope
  | resolves handler via IEventPublisher / Mediator
  v
IEventHandler<TIntegrationEvent>
```

---

## 4. Core Feature Matrix

Status key: DONE = Implemented, PARTIAL = Partial, FIX = Needs Fix, PLANNED = Not yet

| Feature | Category | Status | Priority | Decision |
|---|---|:---:|:---:|---|
| `IEvent` (Id: EventId, OccurredAt: DateTimeOffset) | CORE | DONE | P0 | KEEP |
| `IDomainEvent : IEvent` (marker) | CORE | DONE | P0 | KEEP |
| `IIntegrationEvent : IEvent` (marker) | CORE | DONE | P0 | KEEP |
| `EventId` (readonly record struct, Guid v7) | CORE | DONE | P0 | KEEP |
| `EventType` (readonly record struct, string-semantic) | CORE | DONE | P0 | KEEP |
| `EventVersion` (readonly record struct, uint) | CORE | DONE | P0 | KEEP |
| `CorrelationId` (readonly record struct, IParsable) | CORE | DONE | P0 | KEEP |
| `CausationId` (readonly record struct, IParsable) | CORE | DONE | P0 | KEEP |
| `TenantId` (readonly record struct, IParsable) | CORE | DONE | P0 | KEEP |
| `EventMetadata` (sealed record, FrozenDictionary headers) | CORE | DONE | P0 | KEEP |
| `EventMetadataBuilder` (fluent, zero-allocation) | CORE | DONE | P0 | KEEP |
| `IEventEnvelope` (non-generic, for Outbox/Transport) | CORE | DONE | P0 | KEEP |
| `EventEnvelope<TEvent>` (sealed record) | CORE | DONE | P0 | KEEP |
| `EventEnvelope.Create<TEvent>()` factory | CORE | DONE | P0 | KEEP |
| `EventEnvelope.Wrap<TEvent>()` factory | CORE | DONE | P0 | KEEP |
| `[EventName(string)]` attribute | CORE | DONE | P0 | KEEP |
| `[EventVersion(uint)]` attribute | CORE | DONE | P0 | KEEP |
| `[EventSource(string)]` attribute | CORE | DONE | P0 | KEEP |
| `EventTypeDescriptor` (sealed record) | CORE | DONE | P0 | KEEP |
| `IEventTypeRegistry` (TryGetDescriptor, GetAllDescriptors) | CORE | DONE | P0 | KEEP |
| `EventTypeRegistry` (FrozenDictionary, O(1) lookup) | CORE | DONE | P0 | KEEP |
| `EventTypeRegistryBuilder` (fluent) | CORE | DONE | P0 | KEEP |
| `StaticEventTypeRegistry` (generic cache) | CORE | DONE | P0 | KEEP -- see ADR-021 |
| `IEventHandler<in TEvent>` (ValueTask HandleAsync) | CORE | DONE | P0 | KEEP |
| `IEventPublisher` (ValueTask PublishAsync<TEvent>) | CORE | DONE | P0 | KEEP |
| `IEventSubscriber` (Subscribe/Unsubscribe<TEvent>) | CORE | DONE | P0 | KEEP |
| `EventsDiagnostics` (ActivitySource + Meter, BCL-only) | CORE | DONE | P0 | KEEP |
| `EventValidationException` | CORE | DONE | P1 | KEEP |
| `EventTypeNotFoundException` | CORE | DONE | P1 | KEEP |

---

## 5. Optional Features

### 5.1 In-Process Utility Publisher

| Feature | Category | Status | Priority | Decision |
|---|---|:---:|:---:|---|
| `InMemoryEventPublisher` (ConcurrentDictionary + CopyOnWriteList) | OPTIONAL | DONE | P1 | KEEP in core as utility |

> IMPORTANT: `InMemoryEventPublisher` is a lightweight utility for testing and simple in-process scenarios. It is NOT a production-grade dispatcher. For production, use `EricksonLopez.Mediator`. See ADR-021 (InMemoryPublisher decision).

### 5.2 Source Generator (separate package)

| Feature | Category | Status | Priority | Decision |
|---|---|:---:|:---:|---|
| `EventIncrementalGenerator` (Roslyn Incremental) | OPTIONAL | DONE | P0 | KEEP -- required for full AOT |
| Generated `GeneratedEventRegistry.CreateRegistry()` | OPTIONAL | DONE | P0 | KEEP |
| FQN namespace check for `IEvent` detection | OPTIONAL | DONE | P1 | KEEP -- see ADR-022 |

### 5.3 System.Text.Json Serialization (separate package)

| Feature | Category | Status | Priority | Decision |
|---|---|:---:|:---:|---|
| `EventIdJsonConverter` | OPTIONAL | DONE | P1 | KEEP |
| `EventTypeJsonConverter` | OPTIONAL | DONE | P1 | KEEP |
| `EventVersionJsonConverter` | OPTIONAL | DONE | P1 | KEEP |
| `CorrelationIdJsonConverter` | OPTIONAL | DONE | P1 | KEEP |
| `CausationIdJsonConverter` | OPTIONAL | DONE | P1 | KEEP |
| `TenantIdJsonConverter` | OPTIONAL | DONE | P1 | KEEP |
| `EventMetadataJsonConverter` | OPTIONAL | DONE | P1 | KEEP |
| `EventEnvelopeJsonConverter<TEvent>` | OPTIONAL | DONE | P1 | KEEP |
| `EventsJsonSerializerOptionsExtensions.AddEventsConverters()` | OPTIONAL | DONE | P1 | KEEP |

---

## 6. Ecosystem Features

> These features have been explicitly identified as belonging to other libraries. They will NOT be implemented in `EricksonLopez.Events`.

| Feature | Belongs to | Architectural Reason |
|---|---|---|
| Pipeline behaviors (validation, logging, auth) | `EricksonLopez.Mediator` | Mediator pattern -- not event contract |
| Handler resolution via DI container | `EricksonLopez.Mediator` | DI concern -- not event modeling |
| Request/Response (Command/Query dispatch) | `EricksonLopez.Mediator` | Different message semantics |
| Outbox table schema | `EricksonLopez.Outbox` | DB persistence |
| Transactional publish (same DB tx) | `EricksonLopez.Outbox` | Unit of Work + DB driver |
| Outbox background worker / polling | `EricksonLopez.Outbox` | Background service |
| Value Object base classes | `EricksonLopez.SharedKernel` | DDD shared primitives |
| Entity / Aggregate base classes | `EricksonLopez.SharedKernel` | DDD domain modeling |
| Result / Error modeling | `EricksonLopez.Result` | Cross-cutting concern |

---

## 7. Rejected Features

> Each rejection is backed by an ADR. These features will NOT be implemented regardless of competitive pressure.

| Rejected Feature | ADR | Reason |
|---|---|---|
| Event Bus (global in-process broker) | ADR-001, ADR-017 | `Mediator` responsibility |
| Event Store / Event Sourcing | ADR-001 | Different architectural paradigm |
| Broker drivers (Kafka, RabbitMQ, ASB, SQS) | ADR-001, ADR-018 | Transport concern |
| Transactional Outbox persistence | ADR-018 | `EricksonLopez.Outbox` responsibility |
| Pipeline behaviors / middleware chains | ADR-017 | `EricksonLopez.Mediator` responsibility |
| Retry policies / circuit breaker | ADR-001 | Resilience infrastructure |
| Dead letter queue | ADR-001 | Transport adapter |
| Idempotency / deduplication tracking | ADR-018 | Outbox concern |
| Assembly scanning at runtime | ADR-010, ADR-012 | Violates AOT-first -- use Generator |
| Dynamic proxy / IL emission | ADR-010 | RequiresDynamicCode -- incompatible with AOT |
| `Dictionary<string, object>` metadata | ADR-007 | Boxing + allocation antipattern |
| Saga orchestration | ADR-001 | Workflow engine domain |
| Schema registry integration | ADR-001 | External infrastructure |
| gRPC transport | ADR-001 | Transport adapter |
| EF Core / Dapper integration | ADR-018 | ORM infrastructure |
| ASP.NET Core DI extensions | ADR-001 | Introduces ASP.NET dependency to core |
| Distributed locks | ADR-001 | Infrastructure concern |
| Event sourcing snapshots | ADR-001 | Different paradigm |

---

## 8. Future Features

> These are potential future additions. They are NOT committed for any release.

| Feature | Justification | Condition | Target Package |
|---|---|---|---|
| CloudEvents 1.0 spec adapter | CNCF standard, wide industry adoption | Non-breaking, opt-in | `EricksonLopez.Events.CloudEvents` (new) |
| Protobuf / MessagePack serialization | High-throughput binary serialization | New optional packages | `EricksonLopez.Events.Serialization.*` |
| DDD correctness Roslyn Analyzer | Detect `IDomainEvent` exposed in `IIntegrationEvent` | Complementary to Generator | `EricksonLopez.Events.Generators` |
| `IEventPublisher` overload with `EventEnvelope<TEvent>` | Publish pre-wrapped envelopes | Evaluate if Outbox concern | Core |

---

## 9. Competitive Analysis

> The question is NOT "what does the competitor have?" The question is "is this feature part of our defined responsibility?"

| Feature | EL.Events | MediatR | MassTransit | Wolverine | NServiceBus | Brighter |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| Pure event contracts | Yes | Yes | Yes | Yes | Yes | Yes |
| Guid v7 monotonic identity | Yes | No | No | No | No | No |
| Strongly typed metadata (no Dictionary) | Yes | No | No | No | No | No |
| ISpanFormattable / IUtf8SpanFormattable | Yes | No | No | No | No | No |
| Typed generic envelope | Yes | No | Yes | Yes | Yes | Yes |
| Domain vs Integration event separation | Yes | No | Yes | Yes | Yes | Yes |
| Zero external dependencies (core) | Yes | Yes | No | No | No | No |
| Zero-reflection AOT-first* | Yes* | No | No | No | No | No |
| Roslyn Source Generator | Yes | No | No | Yes | No | No |
| FrozenDictionary for metadata headers | Yes | No | No | No | No | No |
| BCL ActivitySource / Meter observability | Yes | No | Yes | Yes | Yes | No |
| O(1) static event type registry | Yes | No | Yes | Yes | Yes | No |
| Pipeline behaviors | No | Yes | Yes | Yes | Yes | Yes |
| Broker drivers | No | No | Yes | Yes | Yes | Yes |
| Outbox persistence | No | No | Yes | Yes | Yes | Yes |

> *Zero-reflection when using `EricksonLopez.Events.Generators`. Reflection fallback annotated with [RequiresUnreferencedCode]. See ADR-021.

---

## 10. API Surface

The principle: Maximum capability with minimum public API.

```csharp
// EricksonLopez.Events.Contracts
IEvent              { EventId Id; DateTimeOffset OccurredAt; }
IDomainEvent : IEvent
IIntegrationEvent : IEvent
IEventHandler<in TEvent> { ValueTask HandleAsync(TEvent, CancellationToken); }
IEventPublisher { ValueTask PublishAsync<TEvent>(TEvent, CancellationToken); }
IEventSubscriber { Subscribe<TEvent>(...); Unsubscribe<TEvent>(...); }

// EricksonLopez.Events.Identifiers
EventId       (readonly record struct -- Guid v7, ISpanFormattable, IUtf8SpanFormattable)
EventType     (readonly record struct -- string-semantic, IParsable)
EventVersion  (readonly record struct -- uint, IParsable)
CorrelationId (readonly record struct -- string, IParsable)
CausationId   (readonly record struct -- string, IParsable)
TenantId      (readonly record struct -- string, IParsable)

// EricksonLopez.Events.Metadata
EventMetadata        (sealed record -- immutable, FrozenDictionary)
EventMetadataBuilder (sealed class -- fluent, zero-allocation)

// EricksonLopez.Events.Envelopes
IEventEnvelope               (non-generic interface)
EventEnvelope<TEvent>        (sealed record -- typed envelope)
EventEnvelope (static)       { Create<TEvent>(...); Wrap<TEvent>(...); }

// EricksonLopez.Events.Attributes
[EventName(string)]    // semantic name decoupled from CLR
[EventVersion(uint)]   // contract schema version
[EventSource(string)]  // producer source

// EricksonLopez.Events.Registry
EventTypeDescriptor         (sealed record)
IEventTypeRegistry          (TryGetDescriptor x3, GetAllDescriptors)
EventTypeRegistry           (sealed class, FrozenDictionary-backed)
EventTypeRegistryBuilder    (fluent builder)
StaticEventTypeRegistry     (static, generic cache + [RequiresUnreferencedCode] fallback)

// EricksonLopez.Events.Diagnostics
EventsDiagnostics (static class, ActivitySource + Meter, BCL-only)

// EricksonLopez.Events.Exceptions
EventValidationException
EventTypeNotFoundException

// EricksonLopez.Events.Dispatch
InMemoryEventPublisher (sealed class -- utility, not production)
```

---

## 11. Performance Benchmarks

> Benchmarks measured with BenchmarkDotNet on .NET 10.0 (x64 Native / CoreCLR).

| Operation | Mean | Allocated | Notes |
|---|:---:|:---:|---|
| `EventId.New()` (Guid v7) | ~12.4 ns | 0 B | Stack-allocated (Monotonic) |
| `EventId.TryFormat(Span<char>)` | ~8.1 ns | 0 B | Stack-formatted ISpanFormattable |
| `EventEnvelope.Create()` | ~4.2 ns | 1 heap alloc | sealed record (see ADR-023) |
| `EventMetadataBuilder.Build()` | ~45.0 ns | low-alloc | FrozenDictionary build |
| `InMemoryEventPublisher.PublishAsync()` (1 Handler) | ~18.5 ns | 0 B | CopyOnWrite read |
| `InMemoryEventPublisher.PublishAsync()` (50 Handlers) | ~112.0 ns | 0 B | Zero-allocation multi-dispatch |
| `StaticEventTypeRegistry.GetDescriptor<T>()` (Cached) | ~0.0 ns | 0 B | Static generic field read |
| `EventTypeRegistry.TryGetDescriptor()` (N=100) | ~2.1 ns | 0 B | FrozenDictionary O(1) string lookup |

> See benchmarks/EricksonLopez.Events.Benchmarks for reproducible code.

---

## 12. AOT Guarantee Matrix

| Scenario | AOT Safe? | Notes |
|---|:---:|---|
| Using Source Generator + `GeneratedEventRegistry.CreateRegistry()` | 100% | Zero reflection |
| Using `EventTypeRegistry` with manual `Register<T>()` | 100% | Static generic dispatch |
| Using `StaticEventTypeRegistry` with Generator registered | 100% | Generator path |
| Using `StaticEventTypeRegistry` WITHOUT Generator (fallback) | REFLECTION | [RequiresUnreferencedCode] annotated -- see ADR-021 |
| `EventEnvelope<TEvent>` create/read | 100% | No reflection |
| `EventMetadata` + `FrozenDictionary` | 100% | BCL-only |
| `EventsDiagnostics` (ActivitySource + Meter) | 100% | BCL-only |
| `InMemoryEventPublisher.PublishAsync<T>()` | 100% | Generic dispatch |
| STJ serialization with `JsonSerializerContext` | 100% | Source-generated context required |

```bash
# AOT build validation commands
dotnet publish -c Release -r win-x64 -p:PublishAot=true
dotnet publish -c Release -p:PublishTrimmed=true
```

---

## 13. Roadmap

> Phases based on actual code state 2026-08-24. Library is in 1.0.0 release certification.

### Phase A -- AOT Correctness Hardening (P0) -- COMPLETED

Goal: Resolve the ADR-010 vs StaticEventTypeRegistry contradiction found in code audit.

- [x] Annotate `StaticEventTypeRegistry.Cache<T>.ResolveDescriptor()` with [RequiresUnreferencedCode] + [RequiresDynamicCode]
- [x] Fix `EventIncrementalGenerator`: use FQN namespace check instead of `i.Name == "IEvent"` (see ADR-022)
- [x] Correct README.md: EventEnvelope.Create() allocates on heap (not "0 B") -- see ADR-023
- [x] Clarify README.md AOT guarantee: "zero reflection when using the Generator"

### Phase B -- Documentation Completeness (P1) -- COMPLETED

Goal: Complete ADR documentation for all real architectural decisions.

- [x] Create docs/adr/adr-021-static-registry-aot-fallback.md
- [x] Create docs/adr/adr-022-generator-namespace-qualification.md
- [x] Create docs/adr/adr-023-envelope-record-vs-struct.md
- [x] Create docs/adr/adr-024-tenantid-in-core.md
- [x] Update `InMemoryEventPublisher` XML doc: "utility implementation, not a production dispatcher"
- [x] Update ADR-010-aot-strategy.md to reflect Generator vs fallback distinction

### Phase C -- Performance Validation (P1) -- COMPLETED

- [x] Add benchmark: StaticEventTypeRegistry.GetDescriptor<T>() cached vs cold
- [x] Add benchmark: EventTypeRegistry.TryGetDescriptor() with N=1, 10, 100 registered types
- [x] Add multi-handler scenarios: 1, 5, 10, 50 handlers
- [x] Add volume scenarios: 1, 100, 1000 sequential publishes
- [x] Update README and FEATURES with verified numbers

### Phase D -- Production Hardening (P2) -- COMPLETED

- [x] Guide: "Using the Generator for full AOT" (docs/guides/using-generator-for-aot.md)
- [x] Guide: "Integrating with EricksonLopez.Mediator" (docs/guides/integrating-with-mediator.md)
- [x] Guide: "Integrating EventEnvelope with EricksonLopez.Outbox" (docs/guides/integrating-outbox.md)

### Phase E -- CloudEvents Adapter (P1) -- COMPLETED

- [x] Architectural Decision Record created (docs/adr/adr-025-cloudevents-adapter-strategy.md)
- [x] Canonical EventEnvelope<TEvent> to CloudEvents v1.0 attribute specification
- [x] Zero dependency guarantee for core
- [x] Dedicated adapter package created: `EricksonLopez.Events.CloudEvents`
- [x] Bidirectional mapping extensions `ToCloudEvent` and `ToEventEnvelope`
- [x] System.Text.Json AOT configuration extensions and full unit test suite

### Phase F -- Testing Utilities & DDD Analyzers (P2) -- COMPLETED

- [x] Dedicated testing package created: `EricksonLopez.Events.Testing`
- [x] `FakeEventPublisher` with fluent assertions (`ShouldHavePublished`, `ShouldNotHavePublished`, etc.)
- [x] `TestEventHandler` spy and stub for handler execution tracking and simulated failures
- [x] `EventTestBuilder` for fast test envelope creation
- [x] Roslyn DDD Analyzers in `EricksonLopez.Events.Generators`:
  - `ELE001`: Event immutability enforcement
  - `ELE002`: [EventVersion] validation (version >= 1)
  - `ELE003`: [EventName] non-empty validation
  - `ELE004`: [EventSource] non-empty validation
  - `ELE005`: Domain event leakage detection in integration events

### Phase G -- Compile-Time DI Registration Generation (P3) -- COMPLETED

- [x] Extended `EventIncrementalGenerator` to discover `IEventHandler<TEvent>` implementations
- [x] Emits zero-reflection `AddGeneratedEventHandlers(this IServiceCollection, ServiceLifetime)` extensions

---

## 14. ADR Index

| ADR | Title | Status |
|---|---|---|
| ADR-001 | Core Responsibility and Ecosystem Boundaries | Accepted |
| ADR-002 | Separation of Domain Events vs. Integration Events | Accepted |
| ADR-003 | Event Identity with Native Guid Version 7 | Accepted |
| ADR-004 | Explicit Event Type Identity vs. CLR Type Name | Accepted |
| ADR-005 | Monotonic Event Versioning Strategy | Accepted |
| ADR-006 | Typed Event Envelope Design | Accepted |
| ADR-007 | Strongly-Typed Immutable Metadata Model | Accepted |
| ADR-008 | Event Handler Abstraction and Mediator Boundary | Accepted |
| ADR-009 | Event Publication and Dispatching Contracts | Accepted |
| ADR-010 | Native AOT and Trimming Zero-Reflection Guarantee | Accepted |
| ADR-011 | Trimming Annotations and Analyzer Warnings Policy | Accepted |
| ADR-012 | Roslyn Incremental Source Generator for Static Descriptors | Accepted |
| ADR-013 | Serialization Decoupling and System.Text.Json Adapter | Accepted |
| ADR-014 | Package Decomposition Strategy | Accepted |
| ADR-015 | Target Framework Strategy (.NET 10.0 Standard) | Accepted |
| ADR-016 | SharedKernel Boundary and Non-Duplication | Accepted |
| ADR-017 | Mediator Boundary and Responsibility Matrix | Accepted |
| ADR-018 | Outbox Integration Contract and Separation of Persistence | Accepted |
| ADR-019 | Zero-Cost Observability and OpenTelemetry Integration | Accepted |
| ADR-020 | Low Allocation, Zero Boxing and High Throughput Strategy | Accepted |
| ADR-021 | StaticEventTypeRegistry: Reflection Fallback and AOT Honesty | Accepted |
| ADR-022 | EventIncrementalGenerator: Namespace Qualification Check | Accepted |
| ADR-023 | EventEnvelope<TEvent>: record (class) vs record struct | Accepted |
| ADR-024 | TenantId: Presence in Core | Accepted |
| ADR-025 | CloudEvents 1.0 Adapter Strategy and Boundary | Accepted |
| ADR-026 | Testing Naming Convention (Osherove Pattern) and IDE1006 Local Suppression | Accepted |

---

## 15. Package Compatibility Policy

### SemVer Policy

| Change Type | Version |
|---|---|
| Breaking API change (IEvent, contracts, namespaces) | MAJOR (x.0.0) |
| Addition of new public non-breaking types | MINOR (1.x.0) |
| Bug fixes without API change | PATCH (1.0.x) |
| AOT annotation additions ([RequiresUnreferencedCode]) | PATCH (1.0.x) |
| Documentation corrections | PATCH (1.0.x) |
| Source Generator fixes (non-breaking output) | PATCH (1.0.x) |

### Ecosystem Version Coordination

- Independent releases: Changes to EricksonLopez.Events core that do not change contracts consumed by Outbox or Mediator are released independently.
- Coordinated releases: Required when changing IEventEnvelope, IEvent, EventId, or EventMetadata -- as these are consumed by Outbox and Mediator.

### Package Range Policy

Downstream packages should declare:

```xml
<PackageReference Include="EricksonLopez.Events" Version="[1.0.0,2.0.0)" />
```

---

## 16. Testing Strategy

### Coverage Requirements

| Area | Line | Branch | Method | Mutation |
|---|:---:|:---:|:---:|:---:|
| Core library | 100% | 100% | 100% | 100% |
| Serialization.SystemTextJson | 100% | 100% | 100% | 100% |
| Generators | 100% | 100% | 100% | 100% |

### Test Matrix

| Component | Unit | Generator | Benchmark | AOT Build | Trim |
|---|:---:|:---:|:---:|:---:|:---:|
| Event contracts | Yes | -- | -- | Yes | Yes |
| EventId (Guid v7) | Yes | -- | Yes | Yes | Yes |
| EventType, EventVersion | Yes | -- | -- | Yes | Yes |
| CorrelationId, CausationId, TenantId | Yes | -- | -- | Yes | Yes |
| EventMetadata + Builder | Yes | -- | Yes | Yes | Yes |
| EventEnvelope + Factory | Yes | -- | Yes | Yes | Yes |
| EventTypeRegistry | Yes | -- | -- | Yes | Yes |
| StaticEventTypeRegistry (Generator path) | Yes | Yes | -- | Yes | Yes |
| StaticEventTypeRegistry (reflection fallback) | Yes | -- | -- | Annotated | Warning |
| InMemoryEventPublisher | Yes | -- | Yes | Yes | Yes |
| EventsDiagnostics | Yes | -- | -- | Yes | Yes |
| STJ Converters | Yes | -- | Yes | Yes | Yes |
| EventIncrementalGenerator | Yes | Yes | -- | Yes | Yes |

### Gap Tests Required (Phase A)

- StaticEventTypeRegistry fallback path has [RequiresUnreferencedCode] attribute present
- EventIncrementalGenerator does NOT generate for non-EL IEvent from different namespace
- EventEnvelope.Create() allocates on Gen0 heap (benchmark verification)
- Concurrent InMemoryEventPublisher publishing under parallel load

### Toolchain

- Test Framework: xUnit 2.9.3
- Assertions: FluentAssertions 8.0.1
- Property-Based: FsCheck.Xunit 2.16.6
- Mocking: NSubstitute 5.3.0
- Architecture: NetArchTest.eNhancedEdition 1.4.3
- Benchmarks: BenchmarkDotNet 0.14.0
- Mutation: Stryker.NET (threshold: high=100%, low=98%, break=95%)

---

EricksonLopez.Events -- Architecture-first. AOT-first. Performance-first.
Copyright (c) Erickson Lopez. MIT License.
