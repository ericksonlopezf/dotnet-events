# Testing Strategy & Architecture — EricksonLopez.Events

Welcome to the testing documentation for `EricksonLopez.Events`. This document serves as the primary onboarding and architectural reference for testing practices across the repository.

---

## 1. Testing Philosophy & Core Principles

Our testing approach is guided by four foundational tenets:

1. **Pragmatism > Pure Coverage > Dogmatic Purism**: We test observable behavior and domain invariants rather than private implementation details.
2. **Determinism (Zero Flakiness)**: Tests never depend on real wall-clock delays (`Task.Delay`), favorable race conditions, or accidental execution order. Concurrency is coordinated using `TaskCompletionSource`, barriers, or countdown primitives.
3. **FIRST Principles**:
   - **Fast**: In-memory execution, no external databases or network calls.
   - **Independent**: Each test sets up and tears down its own state.
   - **Repeatable**: Produces identical results across developer machines and multi-core CI runners.
   - **Self-Validating**: Clear, descriptive assertions using `AwesomeAssertions`.
   - **Timely**: Written alongside code and maintained with high mutation resistance.
4. **Strict Gatekeeping via Stryker.NET**: We enforce a strict **95% mutation break threshold** (`break: 95`) across all modules without disabling comments on business logic.

---

## 2. Test Suite Organization

Every production library in `src/` has an exact mirror test project in `tests/`:

```text
dotnet-events/
├── src/
│   ├── EricksonLopez.Events/                  ──► tests/EricksonLopez.Events.UnitTests/
│   ├── EricksonLopez.Events.Contracts/        ──► tests/EricksonLopez.Events.ArchitectureTests/
│   ├── EricksonLopez.Events.Generators/       ──► tests/EricksonLopez.Events.Generators.Tests/
│   ├── EricksonLopez.Events.Serialization...  ──► tests/EricksonLopez.Events.Serialization.Tests/
│   ├── EricksonLopez.Events.OpenTelemetry/    ──► tests/EricksonLopez.Events.OpenTelemetry.Tests/
│   ├── EricksonLopez.Events.Inbox/            ──► tests/EricksonLopez.Events.Inbox.Tests/
│   ├── EricksonLopez.Events.Outbox/           ──► tests/EricksonLopez.Events.Outbox.Tests/
│   ├── EricksonLopez.Events.CloudEvents/      ──► tests/EricksonLopez.Events.CloudEvents.Tests/
│   └── EricksonLopez.Events.Testing/          ──► tests/EricksonLopez.Events.Testing.Tests/
└── tests/
    └── EricksonLopez.Events.NativeAotTests/   ──► End-to-end AOT compilation & trimming validation
```

### Test Categorization & Traits

| Test Category | Trait | Purpose | Examples |
|---|---|---|---|
| **Unit Tests** | `[Trait("Category", "Unit")]` | Fast, isolated verification of algorithms, value objects, registries, strategies, and pipelines. | `EventBusTests`, `ExecutionStrategiesTests`, `IdentifiersTests` |
| **Integration Tests** | `[Trait("Category", "Integration")]` | Verification of multi-component interaction with realistic DI containers and diagnostics. | `EventsOpenTelemetryTests`, `EventBusIntegrationTests` |
| **Architecture Tests** | `[Trait("Category", "Architecture")]` | Enforce DDD boundaries, dependency isolation (e.g. core cannot depend on infrastructure), and naming invariants using `NetArchTest.Rules`. | `ArchitectureRulesTests` |
| **Property-Based Tests** | `[Property]` (FsCheck) | Generative property testing over thousands of arbitrary inputs (GUIDs, strings, offsets). | Roundtrip serialization tests, identifier equality, sorting monotonic order. |
| **Roslyn Analyzer Tests** | `[Trait("Category", "Unit")]` | Compile-time syntax/semantic diagnostics and generator verification via `RoslynTestBed`. | `AnalyzerTests`, `EventIncrementalGeneratorTests` |
| **NativeAOT Smoke Tests** | `[Trait("Category", "NativeAOT")]` | Verify zero trim warnings, reflection-free execution, and source generator compatibility. | `NativeAotValidationTests` |

---

## 3. Shared Test Infrastructure & Fixtures

Reusable testing primitives are located in `tests/EricksonLopez.Events.UnitTests/Common/` and the public `EricksonLopez.Events.Testing` package:

### 3.1 `EventBusTestFixture` (Humble Container Object)
Encapsulates a real `ServiceCollection` and `ServiceProvider`, exposing a fluent API for assembling bus configurations without boilerplate:
```csharp
using var fixture = new EventBusTestFixture()
    .WithOptions(opts => opts.ExecutionMode = EventExecutionMode.Sequential)
    .WithEventHandler<OrderPlacedEvent, OrderPlacedHandler>(ServiceLifetime.Scoped)
    .WithEventMiddleware<LoggingMiddleware>(ServiceLifetime.Singleton);

var bus = fixture.GetBus();
await bus.PublishAsync(new OrderPlacedEvent(...));
```

### 3.2 `ActivityTestScope` & `MeterTestScope` (Thread-Safe Diagnostic Listeners)
Wraps OpenTelemetry `ActivityListener` and `MeterListener` in `IDisposable` scopes with internal locks. Eliminates manual listener callbacks while capturing:
- Active/completed activities and their tags (`activityScope.GetActivitiesForOperation(...)`)
- Long and double measurements (`meterScope.LongMeasurements`, `meterScope.DoubleMeasurements`)
- Published instrument metadata (`meterScope.PublishedInstruments`)

### 3.3 `DiagnosticsTestFixture` and `[Collection("Diagnostics")]`
Because `ActivitySource` and `Meter` operate as process-wide static singletons, tests exercising telemetry are grouped into `[Collection("Diagnostics")]` to enforce serial execution and prevent cross-test data bleeding (see [ADR-029](../docs/adr/adr-029-diagnostic-testing-isolation.md)).

### 3.4 `TrackingSynchronizationContext`
A custom `SynchronizationContext` that tracks post/send invocations, used to mathematically verify that all asynchronous continuations specify `.ConfigureAwait(false)` to avoid deadlocks in synchronization-sensitive hosting environments.

### 3.5 Public Testing Package: `EricksonLopez.Events.Testing`
Provides standard doubles for consuming microservices and internal tests (see [ADR-030](../docs/adr/adr-030-public-testing-package.md)):
- `FakeEventPublisher`: In-memory recording publisher with filters and failure injection.
- `TestEventHandler<T>`: Observes and records received events.
- `EventTestBuilder`: Fluent builder for `EventEnvelope<T>` and `EventMetadata`.

---

## 4. Testing Conventions & Best Practices

### 4.1 Test Method Naming (Osherove Tri-Part Pattern)
In accordance with [ADR-026](../docs/adr/adr-026-testing-naming-convention-and-ide1006.md), all test methods follow:
```text
[UnitOfWork]_[StateUnderTest]_[ExpectedBehavior]
```
*Examples:*
- `EventBus_PublishAsync_WithCancelledToken_ShouldThrowOperationCanceledException`
- `SequentialStrategy_WithFailFast_ShouldRethrowImmediatelyAndStopSubsequentHandlers`
- `DomainEventLeakAnalyzer_WhenDomainEventContainsNestedDomainEvent_ShouldNotReportELE005`

### 4.2 AAA Structure
Every test is strictly structured with Arrange, Act, and Assert phases. Avoid multiple interleaved Act/Assert phases in a single test; create separate, focused tests instead.

### 4.3 Strong, Resilient Assertions
- Always assert specific exception types and messages (`.ThrowExactlyAsync<T>().WithMessage("...")`).
- When asserting logger calls, inspect formatted state or arguments (e.g. checking that event type and handler names appear in the warning).
- Avoid `Arg.Any<object>()` when inspecting structured diagnostic payloads.

---

## 5. Mutation Testing with Stryker.NET

Mutation testing validates test assertion quality by introducing syntax mutations. We maintain a modular configuration per package with a **95% break threshold** (see [ADR-031](../docs/adr/adr-031-stryker-mutation-testing-policy.md)).

### Running Stryker Locally

```powershell
# Run mutation testing on core bus
dotnet stryker -c stryker-config.json

# Run mutation testing on source generators & analyzers
dotnet stryker -c stryker-config.generators.json

# Run mutation testing on serialization
dotnet stryker -c stryker-config.serialization.json

# Run mutation testing on inbox / outbox
dotnet stryker -c stryker-config.inbox.json
dotnet stryker -c stryker-config.outbox.json
```

HTML and JSON reports are generated under `StrykerOutput/{timestamp}/reports/mutation-report.html`.

---

## 6. How to Run Tests

### Run the entire test suite via CLI
```powershell
dotnet test
```

### Run tests filtered by category
```powershell
# Run unit tests only
dotnet test --filter "Category=Unit"

# Run integration tests only
dotnet test --filter "Category=Integration"

# Run architecture tests only
dotnet test --filter "Category=Architecture"
```

### Run Performance Benchmarks
BenchmarkDotNet suites are maintained in `benchmarks/EricksonLopez.Events.Benchmarks`:
```powershell
dotnet run -c Release --project benchmarks/EricksonLopez.Events.Benchmarks
```
