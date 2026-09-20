# ADR-032: Built-in Causation Depth Limiting Middleware

## Status
Accepted

## Date
2026-08-28

* **Status:** Accepted
* **Date:** 2026-08-28
* **Deciders:** Architecture Team, Erickson Lopez

---

## Context
In decoupled event-driven architectures, event handlers frequently trigger side-effects that publish secondary or tertiary domain/integration events. In complex domain models, uncoordinated interactions or inadvertent circular event chains (for example, Handler A emits Event B, which triggers Handler B emitting Event A) can cause unbounded recursive dispatch chains. Without runtime recursion limits, this leads to stack overflow exceptions, thread starvation, or memory exhaustion.

## Problem
How should `EricksonLopez.Events` protect applications from cascading event recursion loops while maintaining low-allocation in-process dispatching and composability?

## Options Considered
1. **Thread-Static Recursion Counter:** Rely on thread-local integer counters. (Rejected: Fails across asynchronous `await` continuation boundaries and multi-threaded parallel dispatch).
2. **Hardcoded Check Inside EventBus Core:** Inject recursion logic directly into `EventBus.PublishAsync`. (Rejected: Violates single responsibility principle; bypasses custom pipeline middlewares).
3. **Dedicated Pipeline Middleware (`CausationDepthLimitMiddleware`):** Implement a built-in `IEventMiddleware` interceptor that tracks causation depth along the ambient invocation context.

## Decision
Adopt **Option 3**. Introduce `CausationDepthLimitMiddleware` in `EricksonLopez.Events.Bus.Middleware`. The middleware inspects the ambient dispatch context and ensures the nesting depth does not exceed `EventBusOptions.MaxReentrancyDepth` (default: 10). If the limit is reached, dispatch is aborted immediately with an `InvalidOperationException`.

## Rationale
- Composes natively with the existing `IEventMiddleware` pipeline.
- Operates correctly across asynchronous and parallel dispatch paths.
- Provides configurable thresholds via `EventBusOptions.MaxReentrancyDepth`.
- Preserves zero heap allocations on hot paths by avoiding thread-local allocations.

## Consequences
- **Positive:** Unbounded recursive event publication loops are caught deterministically before crashing the process.
- **Negative:** Deep intentional event cascades exceeding the configured limit will throw `InvalidOperationException` unless `MaxReentrancyDepth` is explicitly increased.

## Related ADRs
- [ADR-007: Distributed Metadata Model](adr-007-metadata-model.md)
- [ADR-008: Handler Ownership & Lifecycle](adr-008-handler-ownership.md)
- [ADR-033: Handler Scope Policy in Parallel Execution](adr-033-handler-scope-policy-in-parallel-dispatch.md)
