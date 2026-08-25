# ADR-002: Separation of Domain Events vs. Integration Events

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
In Domain-Driven Design (DDD), domain events describe facts that occurred strictly within a single Bounded Context / Aggregate root, using the ubiquitous language of the domain. In contrast, integration events are contracts published across network boundaries to communicate state changes to other Bounded Contexts or external subsystems.

## Problem
Should the library use a single marker interface `IEvent` or explicitly bifurcate into `IDomainEvent` and `IIntegrationEvent`?

## Options
1. **Single Marker (`IEvent` only):** Rely entirely on metadata or runtime conventions to distinguish domain vs. integration events.
2. **Distinct First-Class Abstractions (`IDomainEvent` and `IIntegrationEvent` deriving from `IEvent`):** Explicitly separate internal domain semantics from cross-boundary integration contracts.

## Decision
Adopt **Option 2**. We provide:
- `IEvent`: Base contract declaring `EventId Id` and `DateTimeOffset OccurredAt`.
- `IDomainEvent : IEvent`: Represents internal domain transitions (expressed as pure domain records).
- `IIntegrationEvent : IEvent`: Represents versioned, cross-boundary contracts intended for serialization and distribution.

## Rationale
- Prevents leaking internal domain models (aggregates, value objects, internal state) into public distributed message buses.
- Integration events mandate explicit event type naming (`EventType`) and versioning (`EventVersion`), whereas domain events focus on domain semantics.
- Facilitates outbox pattern mapping (`DomainEvent -> Outbox -> IntegrationEvent`).

## Consequences
- **Positive:** Clear architectural intent, type safety at compile-time, zero reflection needed to categorize events.
- **Negative:** Developers must define mapping between domain events and integration events when crossing process boundaries.

## Rejected Alternatives
- Using a single untyped `IEvent` was rejected because it leads developers to expose domain models directly onto message buses, breaking encapsulation and backwards compatibility.
