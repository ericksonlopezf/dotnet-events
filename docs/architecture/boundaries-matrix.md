# Responsibility & Boundaries Matrix

> **Definitive architectural boundary and capability mapping across `EricksonLopez.Events` packages and related ecosystem components.**

---

## 1. Internal Package Boundaries (`EricksonLopez.Events.*`)

| Capability / Concern | `Contracts` | `Events` (Core) | `Generators` | `Serialization` | `CloudEvents` | `OpenTelemetry` | `Inbox` | `Outbox` | `Testing` |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Event Marker Interfaces (`IEvent`, `IDomainEvent`, `IIntegrationEvent`)** | ✅ Primary | Consumes | Analyzes | Consumes | Consumes | Consumes | Consumes | Consumes | Consumes |
| **Monotonic Identifiers (`EventId` Guid v7, `EventType`, `EventVersion`)** | ✅ Primary | Consumes | Analyzes | Serializes | Converts | Tags | Consumes | Consumes | Synthesizes |
| **Ambient Context (`EventMetadata`, `CorrelationId`, `TenantId`)** | ✅ Primary | Consumes | Analyzes | Serializes | Converts | Tags | Filters | Persists | Synthesizes |
| **Typed Transport Envelope (`EventEnvelope<T>`)** | ✅ Primary | Consumes | Analyzes | Serializes | Converts | Instruments | Unwraps | Wraps | Synthesizes |
| **In-Memory Event Bus (`EventBus`, Execution Strategies, Middleware)** | ❌ | ✅ Primary | ❌ | ❌ | ❌ | Instruments | ❌ | ❌ | Fakes |
| **Static & Bidirectional Event Registry** | ❌ | ✅ Primary | Emits | Uses | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Compile-Time Source Generation & Analyzers (`ELE001`–`ELE005`)** | ❌ | ❌ | ✅ Primary | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Native AOT JSON Serialization (`System.Text.Json` Converters)** | ❌ | ❌ | ❌ | ✅ Primary | ❌ | ❌ | ❌ | ❌ | ❌ |
| **CNCF CloudEvents v1.0 Adapter & Converters** | ❌ | ❌ | ❌ | ❌ | ✅ Primary | ❌ | ❌ | ❌ | ❌ |
| **OpenTelemetry Distributed Tracing (`ActivitySource`) & Metrics (`Meter`)**| ❌ | Declares BCL | ❌ | ❌ | ❌ | ✅ Primary OTel | ❌ | ❌ | Scopes |
| **Idempotent Handler Execution & Deduplication Bridge** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ Primary | ❌ | ❌ |
| **Transactional Outbox Event Publisher Bridge** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ Primary | ❌ |
| **Public Test Doubles (`FakeEventPublisher`, `TestEventHandler`)** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ Primary |

---

## 2. Ecosystem Boundary Matrix (`EricksonLopez.*`)

| Architectural Capability | `Events` (This repo) | `Messaging` | `Mediator` | `Outbox` | `SharedKernel` | `MultiTenancy` |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Event Contracts (`IEvent`, `IDomainEvent`, `IIntegrationEvent`)** | ✅ **Primary** | Consumes | Consumes | Consumes | Uses Contracts | ❌ |
| **In-Process In-Memory Event Dispatching** | ✅ **Primary** | ❌ | Pipeline only | ❌ | ❌ | ❌ |
| **Distributed Broker Drivers (RabbitMQ, Kafka, Azure Service Bus)** | ❌ | ✅ **Primary** | ❌ | ❌ | ❌ | ❌ |
| **Network Transport Wire Envelopes & Routing Keys** | ❌ | ✅ **Primary** | ❌ | ❌ | ❌ | ❌ |
| **Command & Query Request-Response Dispatching** | ❌ | ❌ | ✅ **Primary** | ❌ | ❌ | ❌ |
| **Pipeline Behaviors (Validation, Authorization, Caching)** | ❌ | ❌ | ✅ **Primary** | ❌ | ❌ | ❌ |
| **Transactional Outbox Table Persistence & Background CDC** | ❌ | ❌ | ❌ | ✅ **Primary** | ❌ | ❌ |
| **Idempotent Inbox Database Storage & Deduplication State** | ❌ | ❌ | ❌ | ✅ **Primary** | ❌ | ❌ |
| **DDD Aggregate Root Lifecycle Management** | ❌ | ❌ | ❌ | ❌ | ✅ **Primary** | ❌ |
| **Tenant Resolution, Database Isolation & Catalog Engines** | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ **Primary** |

---

## 3. Governance Heuristic (AO-005)

- **`EricksonLopez.Events`** owns event semantics, in-process domain dispatching, and pure data contracts.
- **`EricksonLopez.Messaging`** owns distributed broker transports, routing topologies, network retries, and dead-letter queues ([ADR-028](../adr/adr-028-events-vs-messaging-boundary.md)).
- **`EricksonLopez.Outbox`** owns database storage, transaction coordination, and background polling ([ADR-018](../adr/adr-018-outbox-boundary.md)).
- **`EricksonLopez.MultiTenancy`** owns tenant resolution lifecycles and isolation strategies; `TenantId` in `Events.Contracts` is strictly a routing/partition identifier ([ADR-024](../adr/adr-024-tenantid-in-core.md)).
