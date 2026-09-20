# ADR-034: Native AOT Polymorphic EventEnvelope JSON Converter Factory

## Status
Accepted

## Date
2026-09-01

* **Status:** Accepted
* **Date:** 2026-09-01
* **Deciders:** Architecture Team, Erickson Lopez

---

## Context
Transporting events across network boundaries, serializing to transactional outboxes, or archiving audit logs requires converting `EventEnvelope<TEvent>` to and from JSON using `System.Text.Json`. In Native AOT compiled binaries, polymorphic serialization typically requires either reflection-based `JsonConverterFactory` registrations or extensive `$type` discriminator metadata that breaks trimming safety.

## Problem
How should `EricksonLopez.Events.Serialization.SystemTextJson` serialize generic `EventEnvelope<TEvent>` instances across diverse application events while retaining strict Native AOT trimming safety?

## Options Considered
1. **Reflection-Heavy `JsonConverterFactory` with `Activator.CreateInstance`:** (Rejected: Emits `IL2026` / `IL3050` trim warnings and fails under Native AOT).
2. **Explicit Per-Type Converters Only:** Require consumers to hand-code a dedicated converter for every `EventEnvelope<T>` type. (Rejected: Excessive DX boilerplate).
3. **Compile-Time Generator + Standardized Converter Factory (`EventEnvelopeJsonConverterFactory`):** Supply a factory that instantiates typed converters via generated metadata or explicit registrations with trim annotations.

## Decision
Adopt **Option 3**. Implement `EventEnvelopeJsonConverterFactory` in `EricksonLopez.Events.Serialization.SystemTextJson.Converters`. When registering converters via `services.AddEventsConverters()` or configuring `JsonSerializerOptions`, the factory resolves `EventEnvelope<T>` converters safely, delegating to compile-time generated `JsonSerializerContext` metadata for inner payloads.

## Rationale
- Compliant with .NET 8, 9, and 10 Native AOT trimming requirements.
- Zero runtime code generation (`Reflection.Emit`) or unconstrained generic reflection.
- Integrates seamlessly with standard BCL `System.Text.Json` source generator workflows.

## Consequences
- **Positive:** Full polymorphic serialization support for generic event envelopes without runtime reflection warnings.
- **Negative:** Consuming applications must declare `[JsonSerializable(typeof(EventEnvelope<YourEvent>))]` on their `JsonSerializerContext` partial class.

## Related ADRs
- [ADR-010: Native AOT and Trimming Strategy](adr-010-aot-strategy.md)
- [ADR-013: System.Text.Json Serialization Boundary](adr-013-serialization-boundary.md)
- [ADR-023: Envelope Record vs. Struct Layout](adr-023-envelope-record-vs-struct.md)
