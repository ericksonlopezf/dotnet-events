# ADR-036: Covariant Event Envelope and Envelope Consumer Contracts

## Status
Accepted

## Date
2026-09-04

* **Status:** Accepted
* **Date:** 2026-09-04
* **Deciders:** Architecture Team, Erickson Lopez

---

## Context
Standard event handlers (`IEventHandler<in TEvent>`) receive only the unpacked event payload `TEvent`. While ambient metadata (`CorrelationId`, `CausationId`, `TenantId`) is accessible through `EventContext.Current`, certain architectural use cases (such as multi-tenant routing, outbox relay dispatchers, and audit logging pipelines) require explicit, strongly typed access to the full `EventEnvelope<TEvent>` without relying on `AsyncLocal` state or unboxing non-generic `IEventEnvelope`.

Furthermore, because `EventEnvelope<TEvent>` is an invariant class, consumers could not treat `EventEnvelope<DerivedEvent>` polymorphically as `EventEnvelope<BaseEvent>`.

## Problem
How should `EricksonLopez.Events.Contracts` support direct envelope consumption and polymorphic envelope processing with zero boxing allocations?

## Options Considered
1. **Pass `EventMetadata` as a Secondary Handler Argument:** `HandleAsync(TEvent evt, EventMetadata meta, CancellationToken ct)`. (Rejected: Breaks backward compatibility for all existing handlers).
2. **Require Handlers to Read Ambient `EventContext`:** (Rejected: Does not solve polymorphic envelope passing in transport layers).
3. **Introduce Covariant `IEventEnvelope<out TEvent>` and Dedicated `IEnvelopeEventHandler<in TEvent>`:** Add specialized contracts.

## Decision
Adopt **Option 3**. Introduce:
1. **`IEventEnvelope<out TEvent>`**: A covariant interface implemented by `EventEnvelope<TEvent>`, enabling polymorphic envelope assignments (`IEventEnvelope<IEvent> env = typedEnvelope`).
2. **`IEnvelopeEventHandler<in TEvent>`**: A dedicated handler interface receiving `IEventEnvelope<TEvent>` directly in `HandleAsync(IEventEnvelope<TEvent> envelope, CancellationToken ct)`.
3. **DI Registration Helper**: `services.AddEnvelopeEventHandler<TEvent, THandler>()` in `EricksonLopez.Events.Bus.Extensions`.

## Rationale
- Completely preserves backward compatibility for existing `IEventHandler<TEvent>` consumers.
- Delivers zero-allocation polymorphic envelope handling via covariant generic interface.
- Provides explicit access to transport headers, correlation tokens, and timestamps without ambient context lookups.

## Consequences
- **Positive:** Maximum flexibility for infrastructure bridges, audit loggers, and multi-tenant routers.
- **Negative:** Introduces an additional handler interface that developers must choose between depending on whether they need envelope metadata.

## Related ADRs
- [ADR-006: Event Envelope Structure](adr-006-envelope-design.md)
- [ADR-007: Distributed Metadata Model](adr-007-metadata-model.md)
- [ADR-023: Envelope Record vs. Struct Layout](adr-023-envelope-record-vs-struct.md)
