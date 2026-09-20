# ADR-023: EventEnvelope<TEvent> — record (class) vs record struct

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context

The README documents `EventEnvelope.Create()` as producing "0 B" allocation:

> `EventEnvelope.Create()` | ~4.2 ns | **0 B (record struct wrapper)**

This claim is factually incorrect. `EventEnvelope<TEvent>` is implemented as `sealed record` -- a heap-allocated reference type (class). Each call to `EventEnvelope.Create()` produces one Gen0 heap allocation for the envelope object. The README's claim that it is a "record struct wrapper" is a documentation error.

This ADR formalizes the decision to maintain `EventEnvelope<TEvent>` as a `sealed record` (class) and corrects the documentation.

## Decision

Maintain `EventEnvelope<TEvent>` as a `sealed record` (heap-allocated class reference type). Do not convert to `record struct`. Correct the README benchmark table to reflect the actual allocation behavior.

## Why

`EventEnvelope<TEvent>` contains `EventMetadata` as a field. `EventMetadata` is a `sealed record` (class) that in turn contains:
- `FrozenDictionary<string, string>` (heap-allocated reference type)
- Multiple `string?` fields (heap-allocated reference types)
- `CorrelationId`, `CausationId`, `TenantId` (value types -- stackable)

An envelope containing heap-allocated references cannot be a zero-allocation value type. Converting `EventEnvelope<TEvent>` to `record struct` would:
- Not eliminate heap allocations (the contained `EventMetadata` reference still points to heap)
- Create reference copies on every struct assignment (copies of pointers, not data)
- Require changes to all consumers (breaking change)
- Produce no meaningful performance benefit given that the envelope is constructed once per event boundary crossing

Furthermore, `EventEnvelope<TEvent>` semantics are those of a **transport object**: a single, shared representation of an event-in-flight. Reference semantics (class) are correct for this use case.

## Alternatives Considered

1. **Convert to `record struct`:** Make `EventEnvelope<TEvent>` a value type to match the README claim.
2. **Keep as `sealed record`, fix documentation:** Maintain current implementation and correct the README.
3. **Separate `EventEnvelope` into a struct payload + class metadata wrapper:** Split the envelope into an allocation-free header struct + a class-based metadata holder.

## Why Alternatives Were Rejected

1. **Converting to `record struct`** is a breaking change (changes struct/class semantics for all consumers), does not eliminate heap allocations (EventMetadata is a class), and requires `ref` passing to avoid copies -- which complicates the API significantly. The benefit does not justify the cost.

2. **Keeping as sealed record and fixing documentation** is the correct decision. The implementation is architecturally sound; only the documentation is wrong.

3. **Splitting into struct + class** adds complexity, increases the API surface, and creates confusion about when to use which half. The single-object envelope is simpler and sufficient for all current use cases.

## Consequences

### Positive
- No code changes required -- implementation is already correct
- Documentation accurately reflects measured behavior
- Consumers have correct expectations for memory profiling and GC analysis

### Negative
- The README correction is a minor backward-compatibility concern for users who relied on the "0 B" claim for architecture decisions. This is a documentation fix, not a regression.

### Neutral
- Benchmark results remain valid; only the interpretation column ("0 B") in the README is corrected

## Ecosystem Impact

No impact on `EricksonLopez.Outbox` (already handles `EventEnvelope<TEvent>` as a class reference). No impact on `EricksonLopez.Mediator` or transport adapters.

## Migration

None. Only README documentation update is required. API is unchanged.

## Related Libraries

- `EricksonLopez.Outbox` (persists EventEnvelope -- heap allocation is expected and correct)

## Related ADRs

ADR-006, ADR-020
