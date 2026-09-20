# ADR-033: Handler Scope Resolution Policy in Parallel Execution

## Status
Accepted

## Date
2026-08-30

* **Status:** Accepted
* **Date:** 2026-08-30
* **Deciders:** Architecture Team, Erickson Lopez

---

## Context
When multiple handlers are registered for a single event type and executed under `EventExecutionMode.Parallel`, handlers are invoked concurrently via `Task.WhenAll`. If all parallel handlers resolve scoped dependencies (such as Entity Framework Core `DbContext` or per-request tenant contexts) from the ambient `IServiceProvider` scope, concurrent threads concurrently invoke the same scoped instances. This triggers concurrency violations (e.g. `InvalidOperationException: A second operation was started on this context instance before a previous operation completed`).

## Problem
How should `EricksonLopez.Events` manage DI service scope boundaries during concurrent handler execution without forcing consumers into boilerplate manual scope creation or penalizing sequential dispatch performance?

## Options Considered
1. **Always Reuse Ambient Scope:** Never create child scopes. (Rejected: Fatal concurrency hazards on scoped dependencies in parallel mode).
2. **Always Create Scope Per Handler:** Create a new `IServiceScope` for every handler execution in both sequential and parallel modes. (Rejected: Allocates intermediate scope objects and service instances on hot sequential paths).
3. **Configurable `HandlerScopePolicy` (`Auto`, `CreatePerHandler`, `ReuseAmbientScope`):** Provide explicit policies and sensible defaults.

## Decision
Adopt **Option 3**. Introduce the `HandlerScopePolicy` enum in `EricksonLopez.Events.Bus.Configuration` with values:
- `Auto` (Default): Evaluates execution mode at runtime. Reuses the ambient scope for `Sequential` execution to guarantee 0 B allocations; creates an isolated `IServiceScope` per handler for `Parallel` execution to ensure thread safety.
- `CreatePerHandler`: Explicitly forces child scope creation for every handler regardless of execution mode.
- `ReuseAmbientScope`: Forces reuse of the ambient scope even in parallel execution (documented with concurrency hazard warnings).

## Rationale
- `Auto` delivers the optimal balance: zero allocation overhead for sequential dispatch, and complete thread safety for parallel dispatch.
- Gives developers precise control over DI lifetime management for specialized architectures.
- Fully compatible with Microsoft Dependency Injection and Native AOT.

## Consequences
- **Positive:** Eliminates multi-threaded DbContext access conflicts by default during parallel dispatch without penalizing sequential throughput.
- **Negative:** Parallel execution creates child scopes that must instantiate scoped services independently, incurring predictable per-handler allocations.

## Related ADRs
- [ADR-008: Handler Ownership & Lifecycle](adr-008-handler-ownership.md)
- [ADR-020: Zero-Allocation Performance Strategy](adr-020-performance-strategy.md)
- [ADR-032: Built-in Causation Depth Limiting Middleware](adr-032-causation-depth-limit-middleware.md)
