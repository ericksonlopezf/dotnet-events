# EricksonLopez.Events.Contracts

## 1. Overview
`EricksonLopez.Events.Contracts` is an L0 Foundation package providing pure, immutable contracts and identifiers for domain events and integration events across .NET 8 / 9 / 10 systems.

- **Dependencies**: .NET BCL only (zero `EricksonLopez.*` dependencies).
- **Native AOT**: 100% Native AOT and Trimming compatible (`IsAotCompatible=true`, `IsTrimmable=true`).

---

## 2. Identifier Scoping: TenantId & CorrelationId (ADR-011)

Per **ADR-011**, identifiers defined in this package have explicit, strictly bounded semantics:

1. **`TenantId` (`readonly record struct`)**:
   - **Semantic Scope**: Represents the event correlation and partition routing tenant identity carried inside event envelopes (`EventMetadata`, `EventEnvelope<T>`).
   - **Distinction**: It is **not** a tenant resolution engine or identity manager. The tenant resolution lifecycle, HTTP context parsing, and database isolation policies are sovereignly owned by `EricksonLopez.MultiTenancy`.
2. **`CorrelationId` (`readonly record struct`)**:
   - **Semantic Scope**: A domain value object representing the root causation/correlation chain of an event stream.
   - **Distinction**: It is distinct from `Error.CorrelationId` (a diagnostic string property in `EricksonLopez.Result`).

---

## 3. Core Types
- `IDomainEvent`, `IIntegrationEvent`, `IEvent`
- `EventId` (monotonically sortable GUID v7)
- `CorrelationId`, `CausationId`, `TenantId`
- `EventMetadata`, `EventEnvelope<T>`
- `IEventPublisher`, `IEventSubscriber`, `IEventHandler<TEvent>`, `IEventBus`
