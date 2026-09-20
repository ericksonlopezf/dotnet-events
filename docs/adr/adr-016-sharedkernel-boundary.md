# ADR-016: SharedKernel Boundary and Non-Duplication

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
In large enterprise solutions, `SharedKernel` libraries often end up becoming bloated "dumping grounds" with duplicated definitions of `IEvent`, `IDomainEvent`, or `EventId`.

## Problem
How should `EricksonLopez.Events` relate to `EricksonLopez.SharedKernel` without creating duplicate types or circular dependencies?

## Options
1. **Duplicate interfaces in both packages:** Causes type collision and ambiguity.
2. **Move everything to SharedKernel:** Bloats SharedKernel with messaging concepts.
3. **Strict Ownership in `EricksonLopez.Events`:** `EricksonLopez.Events` owns all event contracts (`IEvent`, `IDomainEvent`, `IIntegrationEvent`, `EventId`, `EventMetadata`). `SharedKernel` can either reference `EricksonLopez.Events` or remain focused on value object base classes, entity base classes, and domain error concepts.

## Decision
Adopt **Option 3**. `EricksonLopez.Events` is the single authoritative owner of all event abstractions. `SharedKernel` must never redefine or shadow these contracts.

## Rationale
- Prevents split-brain type systems.
- Ensures all components across the ecosystem share the exact same `EventId`, `IDomainEvent`, and `IIntegrationEvent` types.

## Consequences
- **Positive:** Absolute consistency across bounded contexts.
- **Negative:** None.

## Rejected Alternatives
- Duplicating interfaces was rejected.
