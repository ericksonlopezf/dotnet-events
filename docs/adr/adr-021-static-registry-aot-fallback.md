# ADR-021: StaticEventTypeRegistry — Reflection Fallback and AOT Honesty

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context

`StaticEventTypeRegistry.Cache<TEvent>` uses a static generic field to cache `EventTypeDescriptor` instances. The field initializer calls `ResolveDescriptor()`, which internally calls `type.GetCustomAttribute<EventNameAttribute>()`, `GetCustomAttribute<EventVersionAttribute>()`, and `GetCustomAttribute<EventSourceAttribute>()`.

This behavior was discovered during the post-implementation code audit and contradicts:

- ADR-010: "Zero runtime reflection guarantee"
- README: "Zero runtime reflection (`Assembly.GetTypes()` and `Type.MakeGenericType` are completely eliminated)"
- The project's primary differentiating claim against MediatR, MassTransit, and Wolverine

The contradiction exists because `GetCustomAttribute<T>()` is a form of runtime reflection. While it does not scan assemblies, it reads attribute metadata that the IL trimmer may eliminate if no other code path preserves it. Under NativeAOT, this can fail silently or produce incorrect results.

## Decision

Annotate `StaticEventTypeRegistry.Cache<TEvent>.ResolveDescriptor()` with `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` to explicitly mark it as a non-AOT-safe reflection path.

Simultaneously, establish a documented two-path model:

1. **Primary path (AOT-safe):** The Source Generator (`EricksonLopez.Events.Generators`) emits `GeneratedEventRegistry` at compile time. Consumers call `GeneratedEventRegistry.CreateRegistry()` at application startup, which populates `StaticEventTypeRegistry.Current` with zero-reflection descriptors.

2. **Fallback path (reflection, non-AOT):** `StaticEventTypeRegistry.Cache<TEvent>.ResolveDescriptor()` maintains its current behavior but is decorated with the appropriate annotations, causing consuming projects without the Generator to receive a compiler warning directing them to the correct approach.

## Why

- The library's advertised guarantee must match the actual code behavior. An unannotated reflection path that silently fails under NativeAOT is an architectural integrity violation.
- `EnableTrimAnalyzer=true` is already active across all production projects. With proper annotation, the trim analyzer will emit warnings at the correct call site -- guiding developers without breaking them.
- Maintaining the reflection fallback preserves DX for simple non-AOT applications that do not need the full Generator setup.

## Alternatives Considered

1. **Remove the fallback entirely:** Force all consumers to use the Source Generator.
2. **Annotate the whole `StaticEventTypeRegistry` class:** Mark the entire class with `[RequiresUnreferencedCode]`.
3. **Replace GetCustomAttribute with typeof(TEvent).Name fallback only:** Remove attribute reading entirely without annotations.

## Why Alternatives Were Rejected

1. **Removing the fallback entirely** would break single-file scripts, simple console apps, and test scenarios that instantiate events without a full Generator pipeline. The DX cost is not justified by the benefit, since proper annotation achieves the same AOT-safety goal.

2. **Annotating the whole class** would be too broad. The `Cache<TEvent>.Descriptor` property getter (which reads the already-cached value) is AOT-safe. Only the initializer `ResolveDescriptor()` is the unsafe path. Annotating the whole class would incorrectly warn on all usages.

3. **Removing attribute reading** was rejected because it removes useful DX for non-AOT projects that do use `[EventName]`, `[EventVersion]`, and `[EventSource]` without the Generator.

## Consequences

### Positive
- The library's AOT guarantee becomes architecturally honest and verifiable
- Consuming projects using NativeAOT without the Generator receive an actionable compiler warning
- No breaking change to the public API -- pure annotation addition
- ADR-010 can be updated to accurately reflect both paths

### Negative
- Projects using `StaticEventTypeRegistry.Cache<TEvent>` without the Source Generator will see a new `IL2026`/`IL3050` warning. This is intentional and correct.
- README and ADR-010 require documentation updates to reflect the two-path model.

### Neutral
- The Source Generator workflow is unchanged
- Annotation is a PATCH-level change (no API surface modification)

## Ecosystem Impact

`EricksonLopez.Mediator` and `EricksonLopez.Outbox` consume `IEventTypeRegistry` but do not call `StaticEventTypeRegistry.Cache<TEvent>` directly. No ecosystem impact on those packages.

## Migration

Consumers who want full AOT compatibility: add `EricksonLopez.Events.Generators` to their project and call `GeneratedEventRegistry.CreateRegistry()` at application startup (or in DI composition root).

Consumers who accept non-AOT: the reflection fallback continues to work exactly as before. The only change is a new compiler warning, which can be suppressed explicitly with `[SuppressMessage]` if intentional.

## Related Libraries

- `EricksonLopez.Events.Generators` (implements the AOT-safe primary path)
- `EricksonLopez.Mediator` (consumes `IEventTypeRegistry`, unaffected)
- `EricksonLopez.Outbox` (consumes `EventEnvelope`, unaffected)

## Related ADRs

ADR-010, ADR-012
