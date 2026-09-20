# 39. ARCHITECTURAL DECISION RECORDS (ADRS EVALUATED)

- **ADR-001**: Use `readonly record struct` for `EventId`, `TenantId`, and `CorrelationId` (Zero-allocation design).
- **ADR-002**: Decouple pure contracts into `EricksonLopez.Events.Contracts` without external dependencies.
- **ADR-003**: Support dual dispatch (`SequentialExecutionStrategy` and `ParallelExecutionStrategy`).
- **ADR-004**: Enforce compile-time immutability via Roslyn Analyzer `ELE001`.
- **ADR-005**: Adopt `System.Text.Json` with Source Generation exclusively for Native AOT compatibility.
- **ADR-006**: Deprecate volatile in-memory buffering (`TransactionalEventPublisher`) in favor of `EricksonLopez.Outbox`.