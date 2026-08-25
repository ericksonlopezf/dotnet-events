# Anti-Feature Matrix (Explicitly Rejected Responsibilities)

The following features have been intentionally excluded from `EricksonLopez.Events`:

| Rejected Feature | Architectural Reason for Rejection | Appropriate Alternative |
| :--- | :--- | :--- |
| **Message Broker Transports (RabbitMQ, Kafka, Azure Service Bus, AWS SQS)** | Transports introduce volatile third-party SDK dependencies, networking logic, and broker-specific configuration. Violates domain cleanliness and core immutability. | Dedicated transport adapter packages (e.g. `EricksonLopez.Events.Transport.Kafka`). |
| **Transactional Outbox Engine (SQL Tables, Polling, CDC)** | Persistence requires database drivers (EF Core, Dapper, Npgsql, SqlClient) and transaction handling. Violates Clean Architecture core rules. | `EricksonLopez.Outbox` |
| **Mediator Pipeline Behaviors & Request-Response Handlers** | Pipeline middlewares (validation, logging, auth filters) and Command/Query routing belong to mediator abstractions. | `EricksonLopez.Mediator` |
| **Distributed Sagas & Orchestration Workflows** | Sagas require persistent state machines, timer engines, and compensation coordinators. | Dedicated workflow engine library. |
| **Dynamic `Dictionary<string, object>` for Metadata** | Causes heap allocations, boxing, type casting exceptions at runtime, and breaks Native AOT. | `EventMetadata` typed struct + `FrozenDictionary<string, string>`. |
| **Runtime Reflection Assembly Scanning (`Assembly.GetTypes()`)** | Assembly scanning degrades startup latency and fails under trimming/Native AOT. | `EricksonLopez.Events.Generators` Roslyn Incremental Generator. |
| **Dynamic Proxy Generation (Castle.Core / Bytecode emission)** | Dynamic IL generation is completely incompatible with Native AOT (`RequiresDynamicCode`). | Roslyn Source Generation & Static Generic Dispatch. |
