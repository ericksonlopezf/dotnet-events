# Architectural Decision Records (ADRs)

This directory documents all architectural decision records for `EricksonLopez.Events`.

---

## 📜 Index of Decisions

| ADR | Title | Status |
|---|---|---|
| [ADR-001](adr-001-core-responsibility.md) | Core Responsibility Boundaries | Accepted |
| [ADR-002](adr-002-domain-vs-integration-events.md) | Domain vs. Integration Events Segregation | Accepted |
| [ADR-003](adr-003-event-identity.md) | Event Identity & UUID Guarantees | Accepted |
| [ADR-004](adr-004-event-type-identity.md) | Canonical Event Type Naming Strategy | Accepted |
| [ADR-005](adr-005-event-versioning.md) | Event Versioning & Schema Evolution | Accepted |
| [ADR-006](adr-006-envelope-design.md) | Event Envelope Structure | Accepted |
| [ADR-007](adr-007-metadata-model.md) | Distributed Metadata Model | Accepted |
| [ADR-008](adr-008-handler-ownership.md) | Handler Ownership & Lifecycle | Accepted |
| [ADR-009](adr-009-publisher-ownership.md) | Publisher Contracts & Dispatch Scope | Accepted |
| [ADR-010](adr-010-aot-strategy.md) | NativeAOT & Trimming Strategy | Accepted |
| [ADR-011](adr-011-trimming-strategy.md) | Trimming Analyzer Enforcement | Accepted |
| [ADR-012](adr-012-source-generation.md) | Roslyn Source Generation Strategy | Accepted |
| [ADR-013](adr-013-serialization-boundary.md) | System.Text.Json Serialization Boundary | Accepted |
| [ADR-014](adr-014-package-decomposition.md) | Package Decomposition & Dependency Flow | Accepted |
| [ADR-015](adr-015-target-frameworks.md) | Multi-Targeting Strategy (.NET 8, 9, 10) | Accepted |
| [ADR-016](adr-016-sharedkernel-boundary.md) | SharedKernel Tier-0 Boundary | Accepted |
| [ADR-017](adr-017-mediator-boundary.md) | Mediator Boundary & Decoupling | Accepted |
| [ADR-018](adr-018-outbox-boundary.md) | Transactional Outbox Boundaries | Accepted |
| [ADR-019](adr-019-observability.md) | OpenTelemetry Activity Tracing | Accepted |
| [ADR-020](adr-020-performance-strategy.md) | Zero-Allocation Performance Strategy | Accepted |
| [ADR-021](adr-021-static-registry-aot-fallback.md) | Static Registry Fallback for AOT | Accepted |
| [ADR-022](adr-022-generator-namespace-qualification.md) | Generator Namespace Qualification | Accepted |
| [ADR-023](adr-023-envelope-record-vs-struct.md) | Envelope Record vs. Struct Layout | Accepted |
| [ADR-024](adr-024-tenantid-in-core.md) | Multi-Tenant Partitioning Key in Core | Accepted |
| [ADR-025](adr-025-cloudevents-adapter-strategy.md) | CloudEvents v1.0 Adapter Strategy | Accepted |
| [ADR-026](adr-026-testing-naming-convention-and-ide1006.md) | Test Method Naming & IDE1006 Policy | Accepted |
| [ADR-027](adr-027-canonical-event-contracts-ownership.md) | Canonical Event Contracts Ownership | Accepted |
| [ADR-028](adr-028-events-vs-messaging-boundary.md) | Events vs. Messaging Broker Boundary | Accepted |
| [ADR-029](adr-029-diagnostic-testing-isolation.md) | Diagnostic Testing Isolation | Accepted |
| [ADR-030](adr-030-public-testing-package.md) | Public Testing Test-Doubles Package | Accepted |
| [ADR-031](adr-031-stryker-mutation-testing-policy.md) | Stryker Mutation Testing Quality Gate | Accepted |
| [REJECT-004](reject-004-global-singleton-event-bus-without-di.md) | Global Singleton EventBus without DI | Rejected |
