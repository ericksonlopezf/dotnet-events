# 01. COMPREHENSIVE FORENSIC SYSTEM INVENTORY

## 1. INTRODUCTION AND SCOPE
This document catalogues the physical topology, assemblies, dependencies, target frameworks, public types, reflection points, and serialization mechanisms across the `EricksonLopez.Events` solution.

---

## 2. PROJECT TOPOLOGY

### A. Production Projects (`src/`)
1. **`EricksonLopez.Events.Contracts`**:
   - **TFM**: `net10.0`
   - **Dependencies**: None (Zero external dependencies).
   - **Role**: Canonical definitions of fundamental abstractions: `IEvent`, `IDomainEvent`, `IIntegrationEvent`, `IEventHandler<T>`, `IEventPublisher`, `EventEnvelope<T>`, `EventId`, `EventType`, `EventVersion`, `CorrelationId`, `CausationId`, `TenantId`, `EventMetadata`.
   - **Signing**: Strongly signed (`EricksonLopez.snk`).

2. **`EricksonLopez.Events`**:
   - **TFM**: `net10.0`
   - **Dependencies**: `EricksonLopez.Events.Contracts`, `Microsoft.Extensions.DependencyInjection.Abstractions`.
   - **Role**: In-memory bus engine, execution strategies (`SequentialExecutionStrategy`, `ParallelExecutionStrategy`), type registry (`EventTypeRegistry`), scope lifecycle policies (`HandlerScopePolicy`), middleware pipeline (`IEventMiddleware`), and volatile transactional publisher (`TransactionalEventPublisher`).

3. **`EricksonLopez.Events.Generators`**:
   - **TFM**: `netstandard2.0` (Roslyn Analyzer / Source Generator)
   - **Dependencies**: `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Analyzers`.
   - **Role**:
     - Analyzer `ELE001` (`EventImmutabilityAnalyzer`): Verifies that types implementing `IEvent` are immutable records (`record` or `record struct`).
     - Source Generator `EventPublisherGenerator`: Generates strongly typed reflection-free dispatchers at compile time.

4. **`EricksonLopez.Events.Serialization.SystemTextJson`**:
   - **TFM**: `net10.0`
   - **Dependencies**: `EricksonLopez.Events.Contracts`, `System.Text.Json`.
   - **Role**: JSON converters for value objects (`EventIdJsonConverter`, `TenantIdJsonConverter`, etc.) and AOT context `EventJsonSerializerContext`.

5. **`EricksonLopez.Events.OpenTelemetry`**:
   - **TFM**: `net10.0`
   - **Dependencies**: `EricksonLopez.Events`, `OpenTelemetry.Api`.
   - **Role**: `ActivitySource` ("EricksonLopez.Events"), W3C `traceparent` propagation, and event telemetry metrics.

6. **`EricksonLopez.Events.CloudEvents`**:
   - **TFM**: `net10.0`
   - **Dependencies**: `EricksonLopez.Events.Contracts`, `CloudNative.CloudEvents`, `CloudNative.CloudEvents.SystemTextJson`.
   - **Role**: Bidirectional mapping between `EventEnvelope<T>` and CloudEvents 1.0 JSON specification.

7. **`EricksonLopez.Events.Testing`**:
   - **TFM**: `net10.0`
   - **Dependencies**: `EricksonLopez.Events.Contracts`.
   - **Role**: In-memory test harnesses and test doubles (`FakeEventPublisher`, `TestEventSink`).

---

### B. Test and Validation Projects (`tests/`)
1. **`EricksonLopez.Events.UnitTests`**: Unit tests for contracts, metadata, identifiers, analyzers, and forensic adversarial tests.
2. **`EricksonLopez.Events.IntegrationTests`**: Bus integration tests with DI container, middleware pipelines, and multi-tenancy.
3. **`EricksonLopez.Events.Generators.Tests`**: Roslyn analyzer verification and source generator tests.
4. **`EricksonLopez.Events.Serialization.SystemTextJson.Tests`**: AOT serialization/deserialization tests and payload fuzzing.
5. **`EricksonLopez.Events.OpenTelemetry.Tests`**: OpenTelemetry activities, traces, and metrics tests.
6. **`EricksonLopez.Events.CloudEvents.Tests`**: CloudEvents 1.0 specification compliance tests.
7. **`EricksonLopez.Events.ArchitectureTests`**: Architecture rules tests with `NetArchTest`.
8. **`EricksonLopez.Events.AotSmokeTest`**: Native AOT console application verifying trimming and runtime invariants in a compiled native binary.

---

### C. Benchmark and Sample Projects
1. **`EricksonLopez.Events.Benchmarks`**: BenchmarkDotNet suite measuring envelope creation, in-memory publishing, zero-alloc TryFormat, registry lookups, and serialization.
2. **`ECommerce.Sample`**: Multi-layer Clean Architecture reference sample showcasing end-to-end integration with DI, Domain events, and Outbox.

---

## 3. API AND DATA STRUCTURE INVENTORY

| Assembly | Namespace | Type / Interface | Modifier | Architectural Role |
| :--- | :--- | :--- | :--- | :--- |
| **Contracts** | `...Contracts` | `IEvent` | `public interface` | Root marker interface for all events. |
| **Contracts** | `...Contracts` | `IDomainEvent` | `public interface` | In-process domain events. |
| **Contracts** | `...Contracts` | `IIntegrationEvent` | `public interface` | Cross-boundary / Outbox integration events. |
| **Contracts** | `...Contracts` | `IEventHandler<in TEvent>` | `public interface` | Typed asynchronous event handler. |
| **Contracts** | `...Contracts` | `IEventPublisher` | `public interface` | Event publisher contract. |
| **Contracts** | `...Identifiers`| `EventId` | `public readonly record struct` | Typed GUID v7 / UUID with `TryFormat`. |
| **Contracts** | `...Identifiers`| `TenantId` | `public readonly record struct` | Strict multi-tenant identifier. |
| **Contracts** | `...Identifiers`| `CorrelationId` | `public readonly record struct` | Distributed trace correlation. |
| **Contracts** | `...Identifiers`| `CausationId` | `public readonly record struct` | Command/event causation chaining. |
| **Contracts** | `...Metadata` | `EventMetadata` | `public sealed record` | Immutable header dictionary. |
| **Contracts** | `...Envelopes` | `EventEnvelope<TEvent>` | `public sealed record` | Envelope holding payload and metadata. |
| **Events** | `...Bus` | `InMemoryEventPublisher` | `public sealed class` | Concurrent in-memory event bus. |
| **Events** | `...Bus` | `EventPublisher` | `public sealed class` | Standard publisher backed by DI. |
| **Events** | `...Registry`| `EventTypeRegistry` | `public sealed class` | Type registry with O(1) lookups. |
| **Events** | `...Registry`| `StaticEventTypeRegistry`| `public static class` | Lock-free static generic type cache. |

---

## 4. REFLECTION AND DYNAMIC CODE
- **Dynamic Code Emission (`Reflection.Emit`)**: None (0 lines).
- **Late-Bound Invocation (`MethodInfo.Invoke`)**: Completely absent on the hot path.
- **Type Activation**: Limited to `ActivatorUtilities.CreateInstance` in DI fallback if consumer registered without concrete type; mitigated with `AddHandler<T>()`.
- **Trimming / AOT Compatibility**: `IsAotCompatible = true`, `<PublishAot>true</PublishAot>` validated.