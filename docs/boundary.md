# Bounded Context & Layer Boundaries

---

## 1. Domain vs. Infrastructure Isolation

```mermaid
classDiagram
    class DomainLayer {
        +IDomainEvent
        +IEvent
    }
    class ApplicationLayer {
        +IEventPublisher
        +IEventHandler
        +EventEnvelope
    }
    class InfrastructureLayer {
        +InMemoryEventPublisher
        +IOutboxStore
        +IInboxStore
        +CloudEventSerializer
    }

    DomainLayer <|-- ApplicationLayer
    ApplicationLayer <|-- InfrastructureLayer
```

- **Domain Layer**: Cannot reference `InMemoryEventPublisher`, Outbox, or CloudEvents serializers.
- **Application Layer**: Deals purely with `IEventPublisher` and `EventEnvelope<T>`.
- **Infrastructure Layer**: Bridges events to database outbox tables or distributed message brokers.
