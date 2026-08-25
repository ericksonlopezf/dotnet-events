# Architectural Decision Record: REJECT-004
## Rejection of Global Static Ambient Event Bus

### Status
**REJECTED (Permanent Directorial Invariant)**

### Context
Proposals were made to introduce a static global singleton `EventBus.Publish(...)` allowing any domain entity to dispatch events without dependency injection or scoped service boundaries.

### Decision
Permanently rejected. Domain events and integration events must be dispatched through scoped `IEventPublisher` / `IEventBus` contracts or outbox collectors to preserve transactional boundaries, multi-tenancy isolation, and deterministic testing.

### Consequences
- Strict ambient tenant and transaction boundary preservation.
- Thread-safety and test isolation guaranteed.
- Zero uncollected memory leaks from static event handler registries.
