# Architecture Overview — EricksonLopez.Events

> **High-throughput, zero-reflection event contracts, in-memory bus orchestration, and ambient primitives for decoupled event-driven .NET applications, architected for trimming and Native AOT.**

---

## 1. Architectural Mission

`EricksonLopez.Events` is the foundational event modeling and dispatching ecosystem within the `EricksonLopez.*` open-source architecture. It provides pure, immutable contracts, monotonically sortable UUIDv7 identifiers, typed ambient metadata, and a zero-reflection in-memory event bus.

It solves the fundamental problem of **architectural coupling**: in traditional systems, domain models often become coupled to message broker transport SDKs (RabbitMQ, Kafka, Azure Service Bus), relational database ORMs, or reflection-heavy in-process mediator pipelines.

```mermaid
graph TD
    subgraph L0["Layer 0: Domain Layer (Pure BCL)"]
        Contracts["EricksonLopez.Events.Contracts<br/><i>IEvent, IDomainEvent, EventId (UUIDv7), EventMetadata</i>"]
    end

    subgraph L1["Layer 1: Application Layer"]
        Core["EricksonLopez.Events<br/><i>EventBus, ExecutionStrategies, Middleware, StaticEventTypeRegistry</i>"]
        Generators["EricksonLopez.Events.Generators<br/><i>Roslyn Incremental Source Generator (ELE001-ELE005)</i>"]
    end

    subgraph L2["Layer 2: Adapters & Extensions"]
        STJ["EricksonLopez.Events.Serialization.SystemTextJson<br/><i>Native AOT STJ Converters</i>"]
        CloudEvents["EricksonLopez.Events.CloudEvents<br/><i>CNCF CloudEvents v1.0 Mappings</i>"]
        OTel["EricksonLopez.Events.OpenTelemetry<br/><i>ActivitySource & Meter Instrumentation</i>"]
        Inbox["EricksonLopez.Events.Inbox<br/><i>Idempotent Consumer Bridge</i>"]
        Outbox["EricksonLopez.Events.Outbox<br/><i>Transactional Outbox Publisher</i>"]
    end

    subgraph TestLayer["Test Infrastructure"]
        Testing["EricksonLopez.Events.Testing<br/><i>FakeEventPublisher, TestEventHandler, EventTestBuilder</i>"]
    end

    Contracts --> Core
    Contracts --> Inbox
    Contracts --> Outbox
    Core --> STJ
    Core --> CloudEvents
    Core --> OTel
    Core --> Testing
    Generators -.->|"Emits GeneratedEventRegistry"| Core
```

---

## 2. Core Architectural Tenets

1. **Native AOT & Trimming First**:
   - Zero runtime reflection in production code paths (`Assembly.GetTypes()` and `Type.MakeGenericType` are strictly eliminated).
   - Roslyn Incremental Generator (`EricksonLopez.Events.Generators`) pre-computes event descriptors and static registries at compile time.
   - Transparent Two-Path model: explicit `[RequiresUnreferencedCode]` annotations for fallback reflection paths ([ADR-021](../adr/adr-021-static-registry-aot-fallback.md)).
2. **Strict Layer Boundary Isolation**:
   - `EricksonLopez.Events.Contracts` depends strictly on the .NET BCL (0 external dependencies).
   - Core dispatchers are purely in-process; network broker mechanics are sovereignly owned by `EricksonLopez.Messaging` ([ADR-028](../adr/adr-028-events-vs-messaging-boundary.md)).
3. **Monotonic Identity & Zero Allocation**:
   - `EventId` is a 16-byte `readonly record struct` powered by native GUID Version 7 (`Guid.CreateVersion7()`).
   - High-order bits contain millisecond Unix timestamps, guaranteeing chronological sorting and eliminating B-Tree index fragmentation in databases ([ADR-003](../adr/adr-003-event-identity.md)).
   - `ISpanFormattable` and `IUtf8SpanFormattable` implementations provide zero-allocation formatting into stack-allocated memory.
4. **Domain Events vs. Integration Events**:
   - `IDomainEvent`: Expresses internal state transitions raised strictly within Aggregate Roots. Kept internal to bounded contexts.
   - `IIntegrationEvent`: Versioned, public contract decorated with `[EventName]`, `[EventVersion]`, and `[EventSource]` published to external services.
5. **Immutable Ambient Metadata**:
   - `EventMetadata` stores `CorrelationId`, `CausationId`, `TenantId`, and custom headers in a `FrozenDictionary<string, string>`, completely eliminating dynamic boxing.

---

## 3. Package Topology & Responsibilities

| Package | Layer | Responsibility | Allowed Dependencies |
| :--- | :---: | :--- | :--- |
| **`EricksonLopez.Events.Contracts`** | L0 | Pure event contracts, `EventId`, `EventType`, `EventVersion`, `EventMetadata`, `EventEnvelope<T>`. | .NET BCL only |
| **`EricksonLopez.Events`** | L1 | In-memory `EventBus`, middleware pipeline, execution strategies, error handling policies. | `Contracts`, `Microsoft.Extensions.*` |
| **`EricksonLopez.Events.Generators`** | Build | Roslyn Incremental Source Generator and analyzers (`ELE001`–`ELE005`). | `Microsoft.CodeAnalysis.CSharp` |
| **`EricksonLopez.Events.Serialization.SystemTextJson`** | L2 | Native AOT JSON converters for identifiers, metadata, and polymorphic envelopes. | `Events` |
| **`EricksonLopez.Events.CloudEvents`** | L2 | Bidirectional mapping between `EventEnvelope<T>` and CNCF `CloudEvent` v1.0. | `Events` |
| **`EricksonLopez.Events.OpenTelemetry`** | L2 | Distributed tracing and metrics instrumentation via `ActivitySource` and `Meter`. | `Events`, `OpenTelemetry` |
| **`EricksonLopez.Events.Inbox`** | L2 | Idempotent consumer decorator guaranteeing exactly-once handler execution. | `Contracts`, `EricksonLopez.Inbox.Abstractions` |
| **`EricksonLopez.Events.Outbox`** | L2 | Transactional outbox publisher bridging `IEventPublisher` to `EricksonLopez.Outbox.Abstractions`. | `Contracts`, `EricksonLopez.Outbox.Abstractions` |
| **`EricksonLopez.Events.Testing`** | Test | Public testing utilities, `FakeEventPublisher`, `TestEventHandler<T>`, `EventTestBuilder`. | `Events` |

---

## 4. In-Process EventBus & Execution Strategies

```mermaid
sequenceDiagram
    autonumber
    actor App as Application Service
    participant Bus as EventBus
    participant Pipe as Middleware Pipeline
    participant Strat as IExecutionStrategy
    participant H1 as Handler 1
    participant H2 as Handler 2
    participant Diag as EventsDiagnostics (ActivitySource/Meter)

    App->>Bus: PublishAsync(event, ct)
    Bus->>Diag: Start Activity & Increment Counter
    Bus->>Pipe: Execute through Middlewares
    Pipe->>Strat: ExecuteAsync(handlers, event, ct)
    alt Sequential Mode
        Strat->>H1: HandleAsync(event, ct)
        Strat->>H2: HandleAsync(event, ct)
    else Parallel Mode
        par Concurrent Dispatch
            Strat->>H1: HandleAsync(event, ct)
        and
            Strat->>H2: HandleAsync(event, ct)
        end
    end
    Strat-->>Pipe: Completed
    Pipe-->>Bus: Pipeline Success
    Bus->>Diag: Complete Activity (Status: Ok)
    Bus-->>App: Completed ValueTask
```

### Execution Strategy Configurations
1. **`EventExecutionMode.Sequential`**:
   - Executes handlers in deterministic order.
   - Respects `ErrorHandlingPolicy.FailFast` (immediate abort on first exception) or `ErrorHandlingPolicy.AggregateAndContinue` (captures all exceptions and throws `EventDispatchException`).
2. **`EventExecutionMode.Parallel`**:
   - Executes all handlers concurrently using `Task.WhenAll`.
   - Aggregates any encountered exceptions into `EventDispatchException`.

---

## 5. Clean Architecture Integration Flow

```mermaid
graph TD
    subgraph Domain["Domain Layer (L0)"]
        Aggregate["Order Aggregate"]
        DomainEvent["OrderCreatedDomainEvent (IDomainEvent)"]
        Aggregate -->|"Raises"| DomainEvent
    end

    subgraph Application["Application Layer (L1)"]
        AppService["OrderAppService"]
        IntegEvent["OrderPlacedIntegrationEvent (IIntegrationEvent)"]
        Envelope["EventEnvelope<OrderPlacedIntegrationEvent>"]
        Bus["IEventBus / IEventPublisher"]

        DomainEvent -->|"Mapped to"| IntegEvent
        IntegEvent -->|"Wrapped in"| Envelope
        AppService -->|"Publishes via"| Bus
    end

    subgraph Infrastructure["Infrastructure Layer (L2)"]
        Outbox["OutboxEventPublisher (EricksonLopez.Events.Outbox)"]
        OutboxStore["Transactional Outbox Table"]
        CloudEvents["CloudEvents Adapter"]

        Bus -->|"Routed to"| Outbox
        Outbox -->|"Persists"| OutboxStore
        OutboxStore -.->|"Serialized via"| CloudEvents
    end
```

---

## 6. Roslyn Incremental Generator Architecture

`EricksonLopez.Events.Generators` analyzes all user-defined types implementing `IEvent`, `IDomainEvent`, or `IIntegrationEvent` at compile time:

```mermaid
graph LR
    Source["C# Source Code"] --> SyntaxProvider["IncrementalValuesProvider<TypeDeclarationSyntax>"]
    SyntaxProvider --> SemanticCheck["FQN Namespace Verification<br/>(EricksonLopez.Events.Contracts.IEvent)"]
    SemanticCheck --> Generator["EventIncrementalGenerator"]
    Generator --> SourceOutput["Emit: GeneratedEventRegistry.g.cs"]
    SourceOutput --> AOTApp["Zero-Reflection Native AOT Executable"]
```

Compile-time analyzers guarantee ecosystem rules before build completion:
- **`ELE001`**: Validates non-empty event name strings in `[EventName]`.
- **`ELE002`**: Validates monotonic version numbers in `[EventVersion]`.
- **`ELE003`**: Validates non-empty event source in `[EventSource]`.
- **`ELE004`**: Enforces immutability on event types (`readonly record struct` or `sealed record`).
- **`ELE005`**: Detects and prevents domain events (`IDomainEvent`) from leaking outside domain boundaries.
