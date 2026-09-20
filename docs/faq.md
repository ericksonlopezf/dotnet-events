# Frequently Asked Questions (FAQ) — EricksonLopez.Events

Answers to common questions about the architecture, scope, capabilities, and differences from other libraries.

---

### How does EricksonLopez.Events differ from MediatR?

- **MediatR** combines commands (1-to-1), queries (1-to-1), and notifications (1-to-N), historically relying on runtime reflection.
- **EricksonLopez.Events** is a library specialized exclusively in **Events** (Domain Events and Integration Events). It offers zero runtime reflection, strict Native AOT and Trimming compatibility, native Guid v7 support, CNCF CloudEvents v1.0 schema conversion, and pure contracts ready for integration with Transactional Outbox and Idempotent Inbox patterns (via `EricksonLopez.Outbox`).

### How does it differ from MassTransit or Wolverine?

- **MassTransit** and **Wolverine** are distributed message transport libraries for external brokers (RabbitMQ, Kafka, Azure Service Bus, Amazon SQS).
- **EricksonLopez.Events** focuses on **in-process dispatch of domain and integration events**. Integration with external brokers is the responsibility of the `EricksonLopez.Messaging` ecosystem.

### Why use Guid v7 for EventId instead of Guid v4?

- **Guid v4** (`Guid.NewGuid()`) is completely random. When used as a primary key in relational databases, it produces high fragmentation of B-Tree index pages.
- **Guid v7** includes a 48-bit UTC timestamp at the beginning of the GUID, making it sequentially sortable in time and optimizing storage and temporal range queries.

### How does the library guarantee 100% Native AOT compatibility?

1. All structs and records use types known to the compiler.
2. Event and handler registration uses compile-time statically generated delegates or explicitly registered DI registrations.
3. Custom converters for `System.Text.Json` are provided that do not depend on reflection.
4. The Roslyn analyzer detects any dynamic patterns not supported by AOT at compile time.

### Can I register multiple handlers for the same event?

Yes. `EventBus` supports an arbitrary number of subscribers for each event type. You can configure whether they execute sequentially (`EventExecutionMode.Sequential`) or concurrently (`EventExecutionMode.Parallel`).
