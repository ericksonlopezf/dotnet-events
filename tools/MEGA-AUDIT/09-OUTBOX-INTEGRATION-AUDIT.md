# 09. OUTBOX PATTERN INTEGRATION AUDIT

## 1. ARCHITECTURAL SEPARATION PRINCIPLE
- **`EricksonLopez.Events`** is responsible for:
  1. Defining canonical event contracts (`IEvent`, `IIntegrationEvent`, `EventEnvelope<T>`).
  2. Managing intra-process routing and dispatching (in-memory bus).
  3. Providing AOT-compatible serialization and metadata enrichment.
- **`EricksonLopez.Outbox`** is responsible for:
  1. Durable relational persistence of events in Outbox tables.
  2. Message deduplication and transactional row locking.
  3. Background worker processing (competing consumers).
  4. Resilience, exponential backoff retries, and Dead Letter Queues (DLQ).

---

## 2. RECOMMENDED INTEGRATION WORKFLOW

When an application requires persistent integration events:
1. Aggregate or command creates an `IIntegrationEvent`.
2. Encapsulated into an `EventEnvelope<T>`.
3. `EricksonLopez.Events.Serialization.SystemTextJson` serializes the envelope to UTF-8 JSON.
4. The serialized payload and metadata headers are handed to `IOutboxStore` for atomic SQL/NoSQL storage.
5. Outbox background workers read records and dispatch to external message brokers (Kafka, RabbitMQ, Azure Service Bus).

This clean separation ensures `EricksonLopez.Events` remains lightweight, devoid of heavy database drivers or broker dependencies.