# ADR-005: Monotonic Event Versioning Strategy

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
As applications evolve, event schemas change (new fields added, deprecated fields removed, data structures altered). Producers and consumers deploy independently, requiring explicit schema versioning to support backward and forward compatibility.

## Problem
How should event versions be represented in the core library and associated with events and envelopes?

## Options
1. **Full SemVer String (`1.2.3-alpha`):** High complexity; minor/patch versions are usually additive and do not warrant routing bifurcations in event systems.
2. **Monotonic Integer `readonly record struct EventVersion(uint Value)` (v1, v2, v3...):** Simple, ultra-fast, zero-allocation, maps directly to major contract revisions.

## Decision
Adopt **Option 2**. Event versions are represented by `EventVersion`, wrapping a `uint` where default/initial is `EventVersion.V1` (value `1`). We also provide an `[EventVersion(1)]` attribute for compile-time metadata extraction.

## Rationale
- Event-driven architectures follow the Tolerant Reader pattern: additive non-breaking changes remain within the same major version; breaking schema transformations increment the major integer version.
- 4-byte value type with zero heap allocations and trivial integer comparison.

## Consequences
- **Positive:** Predictable and simple versioning contract; envelope headers carry clean integer versions.
- **Negative:** Non-breaking additive changes do not produce distinct version identifiers (handled by serializer optionality).

## Rejected Alternatives
- Complex string-based SemVer was rejected due to parsing overhead, heap allocations, and unnecessary complexity for event message routing.
