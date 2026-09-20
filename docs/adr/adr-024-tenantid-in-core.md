# ADR-024: TenantId — First-Class Presence in Core

## Status
Accepted

## Date
2026-08-14

* **Status:** Accepted
* **Date:** 2026-08-14
* **Deciders:** Architecture Team, Erickson Lopez

## Context

`TenantId` is a typed identifier representing the tenant context in which an event occurred. It is defined in `EricksonLopez.Events.Identifiers` as a `readonly record struct` and included as a first-class field in `EventMetadata`.

Its presence in the core contracts library implies that multi-tenancy is a first-class architectural concern in event modeling. This decision was previously implicit; this ADR makes it explicit and justified.

## Decision

Maintain `TenantId` as a `readonly record struct` in `EricksonLopez.Events.Identifiers` and as a first-class, named field in `EventMetadata`. Do not move it to a custom header in `EventMetadata.Headers`.

## Why

Multi-tenancy affects the routing, partitioning, and isolation of events at multiple points in the event pipeline:

1. **Outbox partitioning:** The Outbox pattern may store events in tenant-scoped tables or apply tenant filters during polling
2. **Broker routing:** Message brokers route messages to tenant-specific topics, partitions, or queues
3. **Consumer filtering:** Event consumers filter by `TenantId` to process only events belonging to their tenant
4. **Correlation:** Distributed traces require `TenantId` in span attributes for tenant-scoped observability dashboards

Placing `TenantId` in `EventMetadata.Headers` as a string value (`headers["x-tenant-id"]`) would:
- Require consumers to use magic strings for a universally critical field
- Remove compile-time type safety from a field consumed by every layer of the distributed system
- Make `TenantId` easy to accidentally omit without any compiler feedback

`TenantId.Empty` (backed by `string.Empty`) provides a zero-cost default for single-tenant systems, ensuring no overhead is incurred when multi-tenancy is not used.

## Alternatives Considered

1. **Express TenantId via `EventMetadata.Headers["x-tenant-id"]`:** Use the extensible custom headers dictionary instead of a dedicated field.
2. **Move TenantId to a separate `EricksonLopez.Events.MultiTenancy` package:** Separate the multi-tenancy concern from the core.
3. **Remove TenantId entirely:** Leave it to consuming applications to inject tenant context through custom headers.

## Why Alternatives Were Rejected

1. **Custom headers approach** removes type safety, introduces magic string dependencies in every consumer, and makes the field impossible to validate at compile time. The same reasoning that motivated strongly-typed `CorrelationId` and `CausationId` applies equally to `TenantId`.

2. **Separate package** would create a single-type package, which is the anti-pattern explicitly rejected by ADR-014 (Package Decomposition Strategy). The overhead of adding a dependency for a single struct is not justified.

3. **Removing TenantId entirely** forces every multi-tenant consuming application to re-invent the same pattern (a typed tenant identifier in metadata), creating ecosystem fragmentation.

## Consequences

### Positive
- Type-safe tenant identification available at every layer of the distributed system without custom header lookups
- `TenantId.Empty` provides zero-cost operation for single-tenant applications
- Consistent with `CorrelationId` and `CausationId` in the metadata model -- all distributed context identifiers are first-class typed fields
- Enables strongly-typed routing rules in `EricksonLopez.Outbox` and transport adapters

### Negative
- Single-tenant applications carry a `TenantId` field in every `EventMetadata` instance, backed by `string.Empty`. This is 8 bytes of reference overhead for the string pointer (or zero bytes if the JIT eliminates the empty string allocation due to interning). The cost is negligible.
- SharedKernel must not redefine `TenantId` (enforced by ADR-016).

### Neutral
- Applications that do not use multi-tenancy never need to set `TenantId` -- the builder leaves it as `TenantId.Empty` by default

## Ecosystem Impact

`EricksonLopez.Outbox` benefits directly: it can use `envelope.Metadata.TenantId` for tenant-scoped outbox table routing without parsing custom headers.

`EricksonLopez.Mediator` benefits: pipeline behaviors can filter or route based on `TenantId` without string manipulation.

Transport adapters benefit: they can map `TenantId.Value` to broker-specific tenant headers (e.g., Kafka partition key, RabbitMQ routing key) without string lookup.

## Migration

None. `TenantId` already exists in the current implementation. This ADR formalizes and justifies its presence for future maintainers.

## Related Libraries

- `EricksonLopez.Outbox` (uses TenantId for tenant-scoped outbox routing)
- `EricksonLopez.SharedKernel` (must NOT redefine TenantId per ADR-016)

## Related ADRs

ADR-007, ADR-016
