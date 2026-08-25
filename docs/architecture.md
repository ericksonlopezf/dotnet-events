# System Architecture — EricksonLopez.Events

Technical architecture document, functional component map, responsibility boundaries, and flow diagrams for the **EricksonLopez.Events** ecosystem.

---

## 1. Architectural Design Principles

1. **AOT-First & Zero Reflection**: The dispatch and serialization core does not use runtime reflection (`MakeGenericType`, `Invoke`, `Type.GetType`), guaranteeing 100% compatibility with Native AOT and Trimming on .NET 8+.
2. **Purity of DDD Contracts**: Domain events (`IDomainEvent`) are pure, immutable data structures with no transport, serializer, or mediator dependencies.
3. **Monotonic Identity (Guid v7 on .NET 9+)**: All `EventId` values use `Guid.CreateVersion7()` (RFC 9562) on .NET 9 and later, providing temporal ordering and excellent index performance in storage. On .NET 8, `EventId.New()` falls back to `Guid.NewGuid()` (v4, non-sortable).
4. **Separation of Transport vs. In-Process Dispatch**:
   - **`EricksonLopez.Events`**: Dispatches domain and integration events **within the process**.
   - **`EricksonLopez.Messaging`**: Distributed transport to external message brokers (Kafka, RabbitMQ, Service Bus).

---

## 2. Functional Component Map

```mermaid
graph TD
    subgraph "Domain Layer"
        Agg["Aggregate Root"] -->|Raises| DE["IDomainEvent"]
    end

    subgraph "Application Layer"
        AppSvc["Application Service"] -->|Translates to| IE["IIntegrationEvent"]
        AppSvc -->|Builds| Meta["EventMetadataBuilder"]
        AppSvc -->|Wraps in| Env["EventEnvelope&lt;TEvent&gt;"]
        AppSvc -->|Publishes via| Bus["IEventBus / IEventPublisher"]
    end

    subgraph "Core Dispatch Pipeline (EricksonLopez.Events)"
        Bus --> MW["Middleware Pipeline (IEventMiddleware)"]
        MW --> Strat{"IExecutionStrategy"}
        Strat -->|Sequential| Seq["SequentialExecutionStrategy"]
        Strat -->|Parallel| Par["ParallelExecutionStrategy"]
        Seq --> Reg["HandlerRegistry (No Reflection)"]
        Par --> Reg
        Reg --> Handlers["IEventHandler&lt;TEvent&gt;"]
    end

    subgraph "Ecosystem Bridges & Infrastructure"
        Env -->|Serialize AOT| STJ["EricksonLopez.Events.Serialization.SystemTextJson"]
        Env -->|ToCloudEvent| CE["EricksonLopez.Events.CloudEvents"]
        Bus -->|Persist Transaction| Outbox["EricksonLopez.Events.Outbox"]
        Handlers -->|Idempotent Guard| Inbox["EricksonLopez.Events.Inbox"]
        MW -->|Telemetry Spans| OTEL["EricksonLopez.Events.OpenTelemetry"]
        Bus -..->|Testing Spy/Stub| Test["EricksonLopez.Events.Testing"]
    end
```

---

## 3. Flow and Sequence Diagrams (Mermaid)

### Publication Flow and Middleware Pipeline

```mermaid
sequenceDiagram
    autonumber
    actor Client as Application Service
    participant Bus as EventBus (IEventBus)
    participant MW as Middleware Pipeline
    participant Strat as IExecutionStrategy
    participant Registry as HandlerRegistry
    participant Handler1 as Handler A (Email)
    participant Handler2 as Handler B (Inventory)

    Client->>Bus: PublishAsync(OrderPlacedIntegrationEvent)
    Bus->>MW: InvokeAsync(Event, Delegate)
    activate MW
    Note over MW: Logging / Metrics / Validation
    MW->>Strat: ExecuteAsync(Handlers, Event)
    activate Strat
    Strat->>Registry: GetHandlers(typeof(OrderPlacedIntegrationEvent))
    Registry-->>Strat: [HandlerDescriptor A, HandlerDescriptor B]

    par Concurrent or Sequential
        Strat->>Handler1: HandleAsync(Event)
        Handler1-->>Strat: ValueTask Completed
    and
        Strat->>Handler2: HandleAsync(Event)
        Handler2-->>Strat: ValueTask Completed
    end

    Strat-->>MW: Execution Complete
    deactivate Strat
    MW-->>Bus: Pipeline Complete
    deactivate MW
    Bus-->>Client: ValueTask Completed
```

### Error Handling Flow with `AggregateAndContinue`

```mermaid
sequenceDiagram
    autonumber
    participant Bus as EventBus
    participant Strat as SequentialExecutionStrategy
    participant H1 as Handler A (Success)
    participant H2 as Handler B (Faulty)
    participant H3 as Handler C (Success)

    Bus->>Strat: Execute with Policy: AggregateAndContinue
    Strat->>H1: HandleAsync(Event)
    H1-->>Strat: Success
    Strat->>H2: HandleAsync(Event)
    H2--xStrat: Throws InvalidOperationException
    Note over Strat: Exception recorded in error list
    Strat->>H3: HandleAsync(Event)
    H3-->>Strat: Success
    Strat-->>Bus: Throws EventDispatchException([InnerExceptions])
```

---

## 4. Boundary and Responsibility Matrix

| Package | Primary Responsibility | Dependencies | AOT Support |
|---|---|---|:---:|
| `EricksonLopez.Events.Contracts` | Base contracts, identity structs (Guid v7), interfaces. | BCL only | ✅ 100% |
| `EricksonLopez.Events` | In-memory bus, middlewares, envelopes, thread-safe registry. | Contracts, Microsoft.Extensions.DI | ✅ 100% |
| `EricksonLopez.Events.Serialization.SystemTextJson` | AOT-safe JSON converters for structs and generic envelopes. | System.Text.Json | ✅ 100% |
| `EricksonLopez.Events.Generators` | Roslyn Generator for event discovery and ELE001–ELE005 analyzers. | Microsoft.CodeAnalysis | ✅ 100% |
| `EricksonLopez.Events.CloudEvents` | Bidirectional mapping to CNCF CloudEvents v1.0 specification. | Contracts, System.Text.Json | ✅ 100% |
| `EricksonLopez.Events.OpenTelemetry` | Traces and Metrics OTel instrumentation for the EventBus. | OpenTelemetry.Api | ✅ 100% |
| `EricksonLopez.Events.Inbox` | Idempotent consumer decorator and deduplication. | Contracts, EricksonLopez.Inbox | ✅ 100% |
| `EricksonLopez.Events.Outbox` | Transactional publisher persisted in Outbox. | Contracts, EricksonLopez.Outbox | ✅ 100% |
| `EricksonLopez.Events.Testing` | Test spies, builders, and fluent assertion DSL. | Events | ✅ 100% |
