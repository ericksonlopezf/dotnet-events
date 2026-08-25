# Strategic Roadmap & Milestones

---

## 1. Release Timeline & Capabilities

### v1.0.0 — Production Release (Current)
- Core `IDomainEvent`, `IIntegrationEvent`, and struct `EventEnvelope<T>`.
- `InMemoryEventPublisher` with zero heap allocation combinators.
- CNCF CloudEvents v1.0 schema converter (`EricksonLopez.Events.CloudEvents`).
- Transactional Outbox and Idempotent Inbox abstractions.
- OpenTelemetry Activity tracing and metrics enrichment.
- Compile-time Roslyn source generator for zero-reflection event registries.
- 100% NativeAOT compliance and $\ge 99\%$ test coverage.

### v1.1.0 — Planned Enhancements
- High-performance memory-mapped circular buffer for local IPC event streams.
- Advanced batch event envelope packaging with `ArrayPool<T>` recycling.
- Native gRPC and WebSocket streaming adapters for distributed event propagation.
