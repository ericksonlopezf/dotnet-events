# Level 00 — Conceptual Architecture & Core Boundaries

> **Showcase Level 0** | Reference: `ECommerce.App/Program.cs` (`RunLevel0ConceptualAsync`)

---

## 1. What is EricksonLopez.Events?

**EricksonLopez.Events** is an ultra-fast, zero-allocation in-process event-driven architecture (EDA) ecosystem for modern .NET (`.NET 8`, `.NET 9`, `.NET 10`). Engineered specifically for Domain-Driven Design (DDD), Modular Monoliths, and Clean Architecture microservices, it provides in-process domain dispatching, strongly typed event envelopes, ambient metadata context, and 100% Native AOT trimming safety.

---

## 2. What Problem Does It Solve?

In traditional .NET architectures, mediator libraries and messaging buses introduce critical liabilities:

1. **Heavy Heap Allocations & GC Thrashing**:
   Boxing value types into `object`, wrapping payloads in transient delegate dictionaries, and returning `Task<Unit>` generates garbage collection pressure on high-throughput paths.
2. **Pervasive Runtime Reflection**:
   Scanning assemblies dynamically at startup with `Assembly.GetTypes()` breaks ahead-of-time compilation (Native AOT) and increases cold-start latency in containers.
3. **Loss of Distributed Context & Causality**:
   Ad-hoc event publishing often strips correlation, causation, and tenant identity across domain boundaries.
4. **Conflation of In-Process Dispatch and Broker Transport**:
   Coupling domain event dispatching directly to distributed message broker client SDKs (RabbitMQ, Kafka) introduces dual-write vulnerabilities and distributed failure modes inside database transactions.

---

## 3. Core Architectural Boundaries (ADR-002)

| Layer | Responsibility | Package |
|---|---|---|
| **Domain Events** | Pure business facts emitted by Aggregate Roots within the bounded context. Zero dependencies. | `EricksonLopez.Events.Contracts` |
| **In-Process Dispatch** | Decoupled asynchronous handler invocation via `ValueTask` within the same process memory. | `EricksonLopez.Events` |
| **External Interoperability** | Standardized schema transformation for cross-service event streaming. | `EricksonLopez.Events.CloudEvents` |
| **Distributed Broker Transport** | Physical message delivery over Kafka, RabbitMQ, or Azure Service Bus. | *Out of Scope* (`EricksonLopez.Messaging`) |
| **Distributed Outbox Storage** | Durable database persistence for guaranteed delivery across service boundaries. | *Out of Scope* (`EricksonLopez.Outbox`) |

---

## 4. Package Ecosystem

```
EricksonLopez.Events.Contracts (Zero external dependencies, .NET BCL only)
       ▲
       │
       ├── EricksonLopez.Events (In-process EventBus, Middlewares, DI)
       │         ▲
       │         ├── EricksonLopez.Events.CloudEvents (CNCF CloudEvents v1.0)
       │         ├── EricksonLopez.Events.OpenTelemetry (ActivitySource & Meter)
       │         ├── EricksonLopez.Events.Serialization.SystemTextJson (AOT Converters)
       │         └── EricksonLopez.Events.Testing (FakeEventPublisher, TestEventHandler)
       │
       └── EricksonLopez.Events.Generators (Roslyn compile-time source generator)
```

---

## 5. Pros and Cons vs. Alternatives

| Feature | `EricksonLopez.Events` | MediatR | MassTransit |
|---|---|---|---|
| **Primary Scope** | In-Process Event Dispatch | In-Process Request/Response + Notifications | Distributed Service Bus & Message Broker Transport |
| **Zero Allocation Hot Path** | Yes (`ValueTask`, stackalloc Spans) | No (`Task`, runtime wrapper allocations) | No (Heavy transport pipeline) |
| **Native AOT Compliance** | 100% (Zero runtime reflection, Source Generated) | Partial / Warning-prone | Requires configuration & reflection roots |
| **Identity Standard** | Monotonic GUID Version 7 (`EventId`) | Generic `Guid` or ad-hoc | Internal message IDs |
| **Ambient Metadata Context** | Built-in `EventMetadata` + `EventContext` | Requires custom MediatR pipeline behaviors | Bus message headers |
| **CloudEvents v1.0 Standard** | Native bidirectional mapping | None (Custom mapping required) | Supported via transport plugins |
