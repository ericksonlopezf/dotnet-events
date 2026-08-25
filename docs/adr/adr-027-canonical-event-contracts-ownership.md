# ADR-027: Canonical Event Contracts Ownership (IDomainEvent & IIntegrationEvent)

* **Status:** Accepted
* **Date:** 2026-08-19
* **Deciders:** Architecture Team, Erickson Lopez
* **Related ADRs:** ADR-001, ADR-002, ADR-016, ADR-018; Outbox ADR-021

---

## 1. Context

Across the .NET enterprise ecosystem, multiple packages previously defined overlapping abstractions for domain and integration events:
1. `EricksonLopez.SharedKernel` defined in-memory DDD lifecycle event primitives (`IDomainEvent`).
2. `EricksonLopez.Events.Contracts` defined unified ecosystem event contracts (`IEvent`, `IDomainEvent`, `IIntegrationEvent`, `EventId`).
3. `EricksonLopez.Outbox.Contracts` defined persistence marker interfaces (`IIntegrationEvent`).

This semantic overlap created binary incompatibility across package boundaries, required redundant interface implementations, and violated the core architectural rule: **`ONE CAPABILITY -> ONE OWNER`**.

---

## 2. Decision

1. **`EricksonLopez.Events.Contracts` is declared the sole canonical owner** of all event abstractions across the ecosystem:
   - `IEvent`: Core contract establishing `EventId Id` and `DateTimeOffset OccurredAt`.
   - `IDomainEvent : IEvent`: In-process domain events originating from DDD aggregates.
   - `IIntegrationEvent : IEvent`: Cross-boundary distributed events published via Outbox and Message Brokers.
2. **`EricksonLopez.SharedKernel` remains strictly in Layer L0 (Zero External Dependencies)**:
   - `SharedKernel` retains self-contained, in-process aggregate root lifecycle management and domain events without depending on external messaging infrastructure.
3. **`EricksonLopez.Outbox` is completely decoupled from domain event interfaces (ADR-021)**:
   - `Outbox.Contracts.IIntegrationEvent` is completely removed.
   - `DefaultOutbox` generates UUIDv7 / GUID identifiers unconditionally on write operations.
   - Message routing and persistence rely purely on `[OutboxMessage]` attributes or explicit message metadata without coupling to caller-owned interfaces.

---

## 3. Rationale

- **Zero Ambiguity**: Eliminates split-brain type definitions across packages.
- **Native AOT & Trimming Compliance**: Removing dynamic interface checks in hot persistence paths avoids trimming warnings (IL2075) and reflection overhead.
- **Pure Dependency Graph**: Strictly enforces unidirectional dependency flow (`L0 SharedKernel` $\leftarrow$ `L3 Events` $\leftarrow$ `L4 Outbox / Messaging`).

---

## 4. Consequences

### Positive
- Strict single source of truth for all event abstractions in `EricksonLopez.Events.Contracts`.
- `dotnet-outbox` operates with zero dependency on event contract packages.
- 100% compilation safety and zero warnings under `TreatWarningsAsErrors=true`.

### Negative
- None.
