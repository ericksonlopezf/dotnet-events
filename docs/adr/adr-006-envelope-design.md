# ADR-006: Typed Event Envelope Design

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
When an event is serialized for the outbox, network transmission, or inter-service messaging, it must carry ambient transport metadata (tracing headers, correlation IDs, timestamp, source, schema version) without polluting the pure domain event payload.

## Problem
How should the envelope be modeled to clearly separate the event payload from transport metadata while remaining strongly typed and Native AOT friendly?

## Options
1. **Pollute Event Payload:** Add `CorrelationId`, `CausationId`, `Source`, and `Metadata` directly into every domain event class.
2. **Untyped Generic Envelope (`object Payload`):** Requires boxing value types and runtime type resolution.
3. **Strongly-Typed Generic Envelope (`EventEnvelope<TEvent>`):** Encapsulates typed payload `TEvent` alongside `EventMetadata` and standard envelope headers (`Id`, `Type`, `Version`, `OccurredAt`).

## Decision
Adopt **Option 3**. We provide:
- `IEventEnvelope`: Non-generic interface defining common envelope headers.
- `EventEnvelope<TEvent>`: Generic immutable record implementing `IEventEnvelope` where `TEvent : IEvent`.

## Rationale
- Preserves purity of domain events: domain events only hold domain data.
- Provides a clean, universal wrapper for Outbox persistence, Kafka record values, RabbitMQ message bodies, and CloudEvents mapping.
- 100% type-safe; does not require boxing when `TEvent` is a reference or value type.

## Consequences
- **Positive:** Unambiguous separation of payload vs transport headers.
- **Negative:** Wrapping and unwrapping envelopes occurs at application/transport boundaries.

## Rejected Alternatives
- Untyped object envelopes and polluting domain event classes were rejected.
