# ADR-008: Event Handler Abstraction and Mediator Boundary

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Applications require a mechanism to react to events. However, defining handler execution pipelines, middleware chains, behaviors, request-response handling, and polymorphic routing is the primary responsibility of `EricksonLopez.Mediator`.

## Problem
Should `EricksonLopez.Events` define an `IEventHandler<TEvent>` contract, and if so, what are its limits?

## Options
1. **No handler contract in Events:** Defer all handler concepts to `EricksonLopez.Mediator`.
2. **Minimal Single-Method Handler Contract (`IEventHandler<in TEvent>`):** Provide a pure contract with `ValueTask HandleAsync(TEvent @event, CancellationToken cancellationToken = default)`, without introducing behaviors, interceptors, or complex execution policies.

## Decision
Adopt **Option 2**. We define:
```csharp
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    ValueTask HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
```
All pipeline behaviors, middleware chains, logging decorators, and mediator dispatching remain exclusively in `EricksonLopez.Mediator`.

## Rationale
- Allows consumers who do not use a Mediator library to write standard event consumers with a universal contract.
- Allows `EricksonLopez.Mediator` to implement or adapt `IEventHandler<TEvent>` seamlessly.
- Returns `ValueTask` to minimize task allocations for synchronous or completed handlers.

## Consequences
- **Positive:** Universal handler interface across the ecosystem.
- **Negative:** `EricksonLopez.Events` does not include middleware pipeline execution (by design).

## Rejected Alternatives
- Implementing pipeline behaviors or interceptors inside `EricksonLopez.Events` was rejected to avoid duplicating `Mediator` responsibilities.
