# ADR-022: EventIncrementalGenerator — Namespace Qualification Check

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context

`EventIncrementalGenerator.GetEventModel()` determines whether a type is an event by checking:

```csharp
bool implementsIEvent = symbol.AllInterfaces.Any(static i => i.Name == "IEvent");
```

This uses only the short interface name (`"IEvent"`), without checking the containing namespace. A consuming project that references any other library defining an interface also named `IEvent` in a different namespace (e.g., `SomeOtherFramework.IEvent`, `LegacySystem.Events.IEvent`) would cause the generator to emit `EventTypeDescriptor` entries for unrelated types. This is a correctness defect, not a performance issue.

## Decision

Replace the short-name check with a fully qualified namespace check:

```csharp
bool implementsIEvent = symbol.AllInterfaces.Any(static i =>
    i.Name == "IEvent" &&
    i.ContainingNamespace?.ToDisplayString() == "EricksonLopez.Events.Contracts");
```

This fix also applies to any `IDomainEvent` or `IIntegrationEvent` checks within the generator pipeline.

## Why

- Code generators must be deterministic and correct. False positives in generated code (generating descriptors for non-event types) create runtime errors that are difficult to diagnose because they appear in auto-generated files.
- The additional string comparison is performed at build time in the Roslyn incremental pipeline. Its performance impact is negligible and does not affect IDE responsiveness.
- The fix eliminates an entire class of integration bugs in enterprise solutions that combine multiple frameworks.

## Alternatives Considered

1. **Keep current short-name check with documentation warning:** Document that other `IEvent` interfaces from other frameworks may cause issues.
2. **Check full display string of the interface:** Use `i.ToDisplayString() == "EricksonLopez.Events.Contracts.IEvent"` instead of separate name + namespace checks.
3. **Use SpecialType or metadata token:** Use Roslyn's symbol equality instead of string comparison.

## Why Alternatives Were Rejected

1. **Documenting the limitation** is not a fix. A generator that produces incorrect output in foreseeable scenarios is a defect, not a documented limitation.
2. **Checking the full display string** is equivalent in correctness but is slightly less readable than a two-part check. Not rejected for technical reasons -- both approaches are valid. Chosen approach is more readable and extensible.
3. **Using symbol equality** would require passing the `IEvent` symbol through the generator pipeline, which increases pipeline complexity. For a correctness check on a well-known namespace, string comparison is appropriate.

## Consequences

### Positive
- Generator output is correct even in projects combining multiple frameworks
- Eliminates false positives for types that happen to implement an `IEvent` interface from a different library

### Negative
- None. This is a purely correctness-improving change with no behavioral regression for compliant users.

### Neutral
- The fix is a Patch-level change (no API surface impact, no breaking change to generator output for correct inputs)

## Ecosystem Impact

No impact on `EricksonLopez.Mediator`, `EricksonLopez.Outbox`, or `EricksonLopez.SharedKernel`. The generated `GeneratedEventRegistry` output is identical for projects using only EricksonLopez event types.

## Migration

None required. Projects already using the generator continue to receive identical output.

## Related Libraries

- `EricksonLopez.Events.Generators` (direct fix location)

## Related ADRs

ADR-012
