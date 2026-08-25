# Functional Taxonomy & Architecture Map

---

## 1. Domain Event Lifecycle

```mermaid
sequenceDiagram
    participant Aggregate as Aggregate Root
    participant AppService as Application Service
    participant Publisher as InMemoryEventPublisher
    participant Outbox as Transactional Outbox
    participant Handler as IEventHandler<T>
    participant Telemetry as OpenTelemetry Activity

    Aggregate->>Aggregate: ApplyChange(DomainEvent)
    Aggregate->>AppService: PullDomainEvents()
    AppService->>Publisher: PublishAsync(EventEnvelope)
    Publisher->>Telemetry: Start Activity ("Publish")
    
    par In-Process Execution
        Publisher->>Handler: HandleAsync(Envelope)
    and Persistence
        Publisher->>Outbox: SaveAsync(Envelope)
    end
    
    Publisher->>Telemetry: Record Success / Metrics
```

---

## 2. Layer Boundaries & Dependencies

- **Domain Layer**: References `EricksonLopez.Events.Contracts` only (`IDomainEvent`). Zero infrastructure or broker dependencies.
- **Application Layer**: Dispatches via `IEventPublisher`. Implements `IEventHandler<T>`.
- **Infrastructure Layer**: Implements `IOutboxStore`, `IInboxStore`, and bridges to external message queues (Kafka, Azure Service Bus).
- **Cross-Cutting**: `EricksonLopez.Events.OpenTelemetry` monitors distributed activity propagation.
