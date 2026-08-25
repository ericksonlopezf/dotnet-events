# ADR-029: Isolated Serial Execution for OpenTelemetry Diagnostic Tests via Collection Fixture

* **Status:** Accepted
* **Date:** 2026-08-20
* **Deciders:** Architecture Team, Erickson Lopez

## Context

The `EricksonLopez.Events` library provides native OpenTelemetry integration and observability instrumentation using .NET's standard `System.Diagnostics.ActivitySource` and `System.Diagnostics.Metrics.Meter`.

These diagnostic primitives operate as static/process-wide singletons within the runtime:
1. `EventsDiagnostics.ActivitySource` and `EventBusDiagnostics.ActivitySource` emit trace activities globally.
2. `EventsDiagnostics.Meter` and `EventBusDiagnostics.Meter` publish metrics across the entire application domain.
3. Diagnostic listeners (`ActivityListener` and `MeterListener`) capture events at the process level.

When test runners (such as xUnit) execute test classes concurrently across multiple threads in parallel, tests that subscribe to static `ActivitySource` or `Meter` instances encounter cross-test telemetry bleeding and race conditions:
- Test A publishing `SampleEvent` may inadvertently capture activities or metric increments generated simultaneously by Test B running in parallel on another thread.
- Assertions checking exact counts (e.g., `HaveCount(1)`, `m.Value == 1`) fail intermittently under multi-core CI runners (flakiness).

## Decision

1. **Implement xUnit Test Collection Fixture for Diagnostics:** Define a dedicated xUnit test collection `[CollectionDefinition("Diagnostics", DisableParallelization = true)]` backed by `DiagnosticsTestFixture`.
2. **Apply `[Collection("Diagnostics")]` to All Diagnostic Test Classes:** Every test class that tests `ActivitySource`, `MeterListener`, `ActivityTestScope`, or `MeterTestScope` must declare `[Collection("Diagnostics")]`.
3. **Encapsulate Listener Scopes in IDisposable Fixtures:** Telemetry listeners must be encapsulated inside `ActivityTestScope` and `MeterTestScope` with internal thread locks and deterministic disposal to ensure complete listener unregistration between tests.

## Consequences

### Positive
- **100% Deterministic Diagnostic Tests:** Complete elimination of flaky telemetry assertions caused by cross-test metric/activity leakage.
- **Selective Serialization:** Only diagnostic tests are serialized; the remainder of the unit, architecture, and serialization test suites continue to execute concurrently at maximum parallelism.
- **Resource Hygiene:** `ActivityTestScope` and `MeterTestScope` enforce explicit unsubscription of listeners upon disposal, preventing memory leaks and listener accumulation.

### Negative / Trade-offs
- Diagnostic tests within the `"Diagnostics"` collection run sequentially, increasing total test execution time by a negligible fraction (< 1-2 seconds across the suite).
