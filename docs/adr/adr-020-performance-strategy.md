# ADR-020: Low Allocation, Zero Boxing and High Throughput Strategy

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
High-throughput event-driven microservices process millions of events per second. Heap allocations from identifier wrapping, boxing value types into interfaces, delegate closures, and string formatting trigger garbage collection (GC) pauses that degrade p99 latency.

## Problem
What concrete engineering patterns should `EricksonLopez.Events` enforce to guarantee ultra-low allocation and high throughput?

## Options
1. **Convenience over performance:** Use `class` for all identifiers, `object` dictionaries, LINQ queries, and standard `Task` return types.
2. **Performance-Engineered Baseline:**
   - Value types (`readonly record struct`) for `EventId`, `EventType`, `EventVersion`, `CorrelationId`, `CausationId`, `TenantId`.
   - `ValueTask` for asynchronous handlers.
   - `FrozenDictionary<string, string>` for immutable metadata lookups.
   - Span-based parsing and formatting (`ISpanFormattable`, `IUtf8SpanFormattable`).
   - Zero LINQ in hot paths.

## Decision
Adopt **Option 2**. All foundational primitives are designed for zero heap allocation during creation and formatting. Handler contracts return `ValueTask` to avoid task object allocations when handlers complete synchronously or from caches.

## Rationale
- Minimizes GC Gen0/Gen1 pressure.
- Predictable, sub-microsecond latency profiles.
- Benchmark-driven validation via BenchmarkDotNet.

## Consequences
- **Positive:** Maximum performance and minimal memory footprint.
- **Negative:** Value types should be passed by reference (`in` modifier) when appropriate to prevent unnecessary copying if size exceeds 16 bytes.

## Rejected Alternatives
- Class-based identifiers and mutable `Dictionary<string, object>` were rejected.
