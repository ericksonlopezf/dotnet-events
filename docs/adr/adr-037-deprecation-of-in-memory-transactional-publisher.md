# ADR-037: Removal of In-Memory Transactional Publisher in Favor of Durable Outbox

## Status
Accepted (Superceded by Removal in v2.0.0)

## Date
2026-09-05

* **Status:** Accepted (Implemented: Removed in v2.0.0)
* **Date:** 2026-09-05
* **Deciders:** Architecture Team, Erickson Lopez

---

## Context
In early designs, `ITransactionalEventPublisher` and `TransactionalEventPublisher` were introduced to provide a lightweight in-memory staging buffer. Handlers could call `PublishAsync` to queue events into an internal list, and then call `CommitAndPublishAsync()` to release the buffered events once a primary unit-of-work succeeded.

However, forensic audits identified an inherent **dual-write data loss vulnerability**: because the staging buffer resides exclusively in volatile application RAM, any sudden process termination (container kill, out-of-memory crash, power failure) occurring between the database transaction commit and `CommitAndPublishAsync()` causes the buffered events to vanish permanently without recovery.

## Problem
How should `EricksonLopez.Events` communicate the delivery boundaries of in-memory buffering and guide enterprise consumers toward atomic, durable consistency guarantees?

## Options Considered
1. **Retain In-Memory Transactional Publisher Undocumented:** Leave the API in place. (Rejected: Misleads consumers into believing in-memory buffering provides enterprise transactional guarantees).
2. **Implement an Embedded Relational Outbox Engine in Core:** Add database drivers, table migrations, and SQL persistence to `EricksonLopez.Events`. (Rejected: Violates the Tier-0 zero-dependency architectural boundary).
3. **Deprecate and Remove In-Memory Implementation in Favor of `EricksonLopez.Outbox`:** Remove `ITransactionalEventPublisher` and `TransactionalEventPublisher` completely in v2.0.0, direct consumers to the dedicated transactional outbox library, and enforce zero obsolete code in the core repository.

## Decision
Adopt **Option 3**. Completely remove `ITransactionalEventPublisher` and `TransactionalEventPublisher` from `EricksonLopez.Events` in the `v2.0.0` major release. Update all documentation and cookbooks to explicitly state that in-process dispatch is strictly **At-Most-Once**, and that any application requiring guaranteed **At-Least-Once** durability across process crashes must use `EricksonLopez.Outbox`.

## Rationale
- Upholds architectural transparency and prevents catastrophic data loss in production.
- Preserves the purity of `EricksonLopez.Events` as an in-process, zero-allocation dispatch engine.
- Enforces the repository's zero-tolerance policy against obsolete APIs in active production code.
- Directs consumers to the proper transactional outbox pattern implemented with persistent storage.

## Consequences
- **Positive:** Clear architectural boundaries, zero obsolete code in production, 100% compliance with zero-obsolete policy.
- **Negative:** Breaking change for v1.x consumers utilizing `ITransactionalEventPublisher`; consumers requiring transactional staging must migrate to `EricksonLopez.Outbox` or dispatch events post-commit.

## Related ADRs
- [ADR-001: Core Responsibility Boundaries](adr-001-core-responsibility.md)
- [ADR-018: Transactional Outbox Boundaries](adr-018-outbox-boundary.md)
- [ADR-028: Events vs. Messaging Broker Boundary](adr-028-events-vs-messaging-boundary.md)
