# ADR-013: Serialization Decoupling and System.Text.Json Adapter

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Events must be serialized for persistence (Outbox) and network transport (Kafka, RabbitMQ). However, coupling the core domain event contracts directly to a specific JSON serializer (or forcing dependencies like Newtonsoft.Json or System.Text.Json options) pollutes the core.

## Problem
How should serialization be structured to keep the core pure while providing high-performance, Native AOT-friendly serialization support?

## Options
1. **Core includes System.Text.Json attributes directly:** Fast to implement, but couples core contracts to System.Text.Json.
2. **Abstract Core + Dedicated Integration Package (`EricksonLopez.Events.Serialization.SystemTextJson`):**
   - Core defines abstract contracts (`IEventSerializer` or wire concepts).
   - Integration package provides Native AOT `JsonConverter` implementations for `EventId`, `EventType`, `EventVersion`, `CorrelationId`, `CausationId`, `TenantId`, `EventMetadata`, and `EventEnvelope<TEvent>`.

## Decision
Adopt **Option 2**. Core defines contracts and value types. The companion package `EricksonLopez.Events.Serialization.SystemTextJson` provides custom AOT converters, source-generated `JsonSerializerContext` helpers, and polymorphic envelope deserialization.

## Rationale
- Keeps `EricksonLopez.Events` core package pure and dependency-free.
- Allows full customization of JSON casing, snake_case, camelCase, and CloudEvents compliance in the adapter.
- Native AOT compliant using `JsonSerializerContext` and `IJsonTypeInfoResolver`.

## Consequences
- **Positive:** Maximum architectural cleanliness and modularity.
- **Negative:** Projects doing JSON serialization add `EricksonLopez.Events.Serialization.SystemTextJson`.

## Rejected Alternatives
- Hardcoding JSON attributes into core record contracts was rejected.
