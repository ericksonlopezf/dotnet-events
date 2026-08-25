# ADR-017: Mediator Boundary and Responsibility Matrix

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Developers often confuse in-process Mediator notification dispatching (1-to-many publish with pipeline behaviors, validation, authorization, logging) with event contracts and envelope modeling.

## Problem
Where does `EricksonLopez.Events` end and where does `EricksonLopez.Mediator` begin?

## Options
1. **Combine Mediator and Events in one library:** Violates Single Responsibility and complicates AOT compilation.
2. **Distinct Architectural Layers:**
   - `EricksonLopez.Events`: Defines what an event is (`IEvent`, `IDomainEvent`, `IIntegrationEvent`, `EventEnvelope`, `EventMetadata`, `EventId`, `EventType`) and a minimal publisher interface (`IEventPublisher`).
   - `EricksonLopez.Mediator`: Implements request/response, command handling, query handling, streaming, and pipeline behaviors (logging, validation, caching, metrics).

## Decision
Adopt **Option 2**. `EricksonLopez.Events` provides minimal in-process event dispatching (`EventBus`, `InMemoryEventPublisher`) without pipeline behaviors, validation filters, or command/query abstractions. `EricksonLopez.Mediator` provides full-featured mediation (pipeline behaviors, request/response, CQRS) and may implement `IEventPublisher` or replace `EventBus` for applications requiring cross-cutting concerns beyond middleware.

## Rationale
- Pure separation of concerns.
- Projects that only need to define domain events, serialize integration events, or perform lightweight in-process dispatch do not need a mediator framework.

## Consequences
- **Positive:** Unambiguous boundaries, zero feature duplication.
- **Negative:** None.

## Rejected Alternatives
- Adding MediatR-style pipeline behaviors to `Events` was rejected.
