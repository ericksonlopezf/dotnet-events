# Strategic Roadmap & Milestones

---

## 1. Release Timeline & Milestones

### v1.0.0 — Initial Production Release (Tagged)
- Core event contracts: `IEvent`, `IDomainEvent`, `IIntegrationEvent`, `IEventEnvelope<T>`, and `EventEnvelope<T>`.
- In-process `EventBus` dispatch engine supporting sequential and parallel execution strategies.
- Extensible pipeline middleware architecture (`IEventMiddleware`).
- CNCF CloudEvents v1.0 bidirectional adapter (`EricksonLopez.Events.CloudEvents`).
- Native AOT System.Text.Json serialization converters (`EricksonLopez.Events.Serialization.SystemTextJson`).
- Compile-time Roslyn source generator and diagnostics analyzers `ELE001`–`ELE005` (`EricksonLopez.Events.Generators`).
- OpenTelemetry W3C distributed tracing and metrics instrumentation (`EricksonLopez.Events.OpenTelemetry`).
- Comprehensive testing harness with `FakeEventPublisher` and assertions (`EricksonLopez.Events.Testing`).

### v2.0.0 — Production Release (2026-09-20)
- **Covariant Event Envelope**: `IEventEnvelope<out TEvent>` enabling polymorphic collection handling ([ADR-036](adr/adr-036-covariant-event-envelope-and-envelope-handler.md)).
- **Direct Envelope Handlers**: `IEnvelopeEventHandler<in TEvent>` providing full metadata access in subscriber handlers without unboxing ([ADR-036](adr/adr-036-covariant-event-envelope-and-envelope-handler.md)).
- **Configurable Handler Scope Policy**: `HandlerScopePolicy` (`Auto`, `CreatePerHandler`, `ReuseAmbientScope`) for parallel execution safety ([ADR-033](adr/adr-033-handler-scope-policy-in-parallel-dispatch.md)).
- **Causation Depth Limiting**: `CausationDepthLimitMiddleware` protecting against infinite cascading event loops ([ADR-032](adr/adr-032-causation-depth-limit-middleware.md)).
- **Polymorphic Envelope JSON Converter**: `EventEnvelopeJsonConverterFactory` enabling dynamic Native AOT serialization ([ADR-034](adr/adr-034-polymorphic-envelope-json-converter.md)).
- **Benchmark Regression Gate**: Automated CI quality gate enforcing 0 B heap allocation invariant and $\le 5\%$ latency degradation ([ADR-035](adr/adr-035-benchmark-regression-gate-and-zero-allocation.md)).
- **Native AOT Smoke Harness**: Dedicated executable `tests/EricksonLopez.Events.AotSmokeTest` running on Windows and Linux CI runners ([ADR-010](adr/adr-010-aot-strategy.md)).
- **Modular Mutation Testing**: Standardization across 7 package-specific Stryker configuration files with 95% break threshold ([ADR-031](adr/adr-031-stryker-mutation-testing-policy.md)).
- **Removal of In-Memory Transactional Publisher**: Complete removal of volatile in-memory `ITransactionalEventPublisher` and `TransactionalEventPublisher` from core, standardizing all outbox persistence on `EricksonLopez.Outbox` ([ADR-037](adr/adr-037-deprecation-of-in-memory-transactional-publisher.md)).
- **Roslyn Immutability Analyzer (`ELE006`)**: Diagnostic analyzer warning on mutable collection types in event contracts.

