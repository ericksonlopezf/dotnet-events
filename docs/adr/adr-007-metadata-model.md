# ADR-007: Strongly-Typed Immutable Metadata Model

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Events in transit need to carry metadata such as correlation identifiers, causation identifiers, tenant identifiers, source services, and optional custom headers. A common anti-pattern in messaging frameworks is using `Dictionary<string, object>`, which causes heavy allocations, boxing, thread-safety issues, and runtime type conversion exceptions.

## Problem
How can metadata be represented in an immutable, low-allocation, Native AOT-safe manner?

## Options
1. **Dynamic `Dictionary<string, object>`:** Allocates on heap, boxes all value types (Guids, ints, dates), no compile-time typing, mutable.
2. **`EventMetadata` readonly struct / record with strongly typed first-class properties:** Explicit fields for `CorrelationId`, `CausationId`, `TenantId`, `Source`, `SchemaVersion`, `ContentType`, plus an optional immutable `FrozenDictionary<string, string>` or typed string map for arbitrary custom headers.

## Decision
Adopt **Option 2**. `EventMetadata` is an immutable type containing first-class properties for common enterprise concerns and an immutable dictionary (`IReadOnlyDictionary<string, string>` / `FrozenDictionary<string, string>`) for custom headers. We provide a fluent, zero-allocation builder pattern (`EventMetadataBuilder`) for constructing metadata.

## Rationale
- Zero boxing for standard identifiers (`CorrelationId`, `CausationId`, `TenantId`).
- Immutability guarantees thread safety across async tasks.
- String-based custom headers guarantee safe wire serialization across polyglot transports.

## Consequences
- **Positive:** Predictable memory footprint, high performance, AOT compatibility.
- **Negative:** Custom headers must be represented as strings (or serialized values).

## Rejected Alternatives
- Untyped object dictionaries (`Dictionary<string, object>`) were rejected.
