# Clean Architecture Integration Guide

## 1. Project Dependencies in Clean Architecture

```text
Domain Project (Core)
  └── EricksonLopez.Events (Defines IDomainEvent, EventId)

Application Project
  └── EricksonLopez.Events (Defines IIntegrationEvent, EventEnvelope, IEventHandler, IEventPublisher)

Infrastructure Project
  ├── EricksonLopez.Events.Serialization.SystemTextJson
  └── Transport / Outbox Drivers (Kafka, RabbitMQ, EF Core)
```

## 2. Purity Rules
- The **Domain** must NEVER reference `EricksonLopez.Events.Serialization.SystemTextJson` or any transport packages.
- Domain Entities / Aggregates raise `IDomainEvent`.
- Application Services or Outbox Interceptors convert `IDomainEvent` into `IIntegrationEvent` wrapped in `EventEnvelope<TIntegrationEvent>`.
- Infrastructure handles wire formatting and persistence.
