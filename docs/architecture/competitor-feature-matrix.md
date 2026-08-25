# Competitor Feature Matrix

| Feature | `EricksonLopez.Events` | MassTransit | MediatR | Wolverine | Brighter | Rebus | Decision |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **Pure Event Contracts (`IEvent`)** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | **KEEP** |
| **Native Guid v7 `EventId`** | ✅ | ❌ (v4/str) | ❌ | ❌ | ❌ | ❌ | **KEEP** |
| **Strongly-Typed Low-Alloc Metadata** | ✅ | ❌ (`Dictionary<string, object>`) | ❌ | ❌ | ❌ | ❌ | **KEEP** |
| **Typed Event Envelope (`EventEnvelope<T>`)** | ✅ | ✅ (Envelope) | ❌ | ✅ | ✅ | ✅ | **KEEP** |
| **AOT Incremental Source Generator** | ✅ | ❌ | ❌ | ✅ (Pre-compiled) | ❌ | ❌ | **KEEP** |
| **Zero External Dependencies in Core** | ✅ (0 dep) | ❌ (Heavy) | ✅ | ❌ | ❌ | ❌ | **KEEP** |
| **Zero Runtime Reflection Guarantee** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | **KEEP** |
| **Pipeline Behaviors / Interceptors** | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | **REJECT (Mediator)** |
| **Broker Drivers (RabbitMQ, Kafka, SQS)** | ❌ | ✅ | ❌ | ✅ | ✅ | ✅ | **REJECT (Transport)** |
| **Transactional Outbox Storage** | ❌ | ✅ | ❌ | ✅ | ✅ | ✅ | **REJECT (Outbox)** |
| **Sagas / State Machines** | ❌ | ✅ | ❌ | ✅ | ❌ | ✅ | **REJECT (Workflow)** |
| **Retry & Circuit Breaker Policies** | ❌ | ✅ | ❌ | ✅ | ✅ | ✅ | **REJECT (Resilience)** |
