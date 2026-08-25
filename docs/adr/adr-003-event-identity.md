# ADR-003: Event Identity with Native Guid Version 7

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context
Every event requires a globally unique identifier for tracing, deduplication (idempotency checking in inboxes/outboxes), and chronological ordering. Historically, libraries used random `Guid` (v4), `Ulid`, or arbitrary strings.

## Problem
What underlying representation should `EventId` use in .NET 10 to balance performance, zero allocation, sortability, database indexing efficiency, and Native AOT compatibility?

## Options
1. **Plain `string`:** High flexibility, but incurs heap allocations, variable lengths, and higher memory overhead.
2. **Random `Guid` (v4):** 16 bytes and zero-alloc, but causes B-Tree index fragmentation in relational databases.
3. **External `Ulid` package:** Time-sortable, but introduces a non-standard 3rd party dependency.
4. **`readonly record struct EventId(Guid Value)` with native `Guid.CreateVersion7()`:** Built into the .NET 10 BCL, time-sortable (millisecond precision timestamp prefix + cryptographically random bits), 16 bytes value type, zero allocation, excellent B-Tree indexing.

## Decision
Adopt **Option 4**. `EventId` is implemented as a strongly typed `readonly record struct EventId(Guid Value)` leveraging `Guid.CreateVersion7()` as its default generator.

## Rationale
- 100% BCL standard without external NuGet dependencies.
- Natural chronological sortability (`IComparable<EventId>`).
- Zero heap allocation on creation and pass-by-value ergonomics.
- Efficient storage and indexing in PostgreSQL (UUIDv7), SQL Server, SQLite, MongoDB, etc.
- Implements `ISpanFormattable`, `IUtf8SpanFormattable`, and `IParsable<EventId>` for AOT-friendly zero-allocation parsing and stringification.

## Consequences
- **Positive:** Natural chronological sortability (`IComparable<EventId>`), zero dependencies, zero heap pressure.
- **Caveat:** `Guid.CreateVersion7()` requires .NET 9+. On .NET 8 (also a supported TFM), `EventId.New()` falls back to `Guid.NewGuid()` (RFC 4122 v4, random, non-time-sortable). Consumers targeting .NET 8 must be aware that `EventId` values will not be chronologically ordered.

## Rejected Alternatives
- String-based IDs and 3rd party Ulid packages were rejected to avoid heap allocations and external dependency baggage.
