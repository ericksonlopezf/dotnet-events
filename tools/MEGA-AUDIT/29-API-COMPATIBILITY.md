# 29. PUBLIC API EVOLUTION AUDIT

## 1. STABLE PUBLIC API INVENTORY
- `IEvent`, `IDomainEvent`, `IIntegrationEvent`
- `IEventHandler<TEvent>`
- `IEventPublisher`
- `EventEnvelope<TEvent>`
- `EventId`, `TenantId`, `CorrelationId`, `CausationId`, `EventType`, `EventVersion`
- `EventMetadata`
- `services.AddEventBus(...)`

---

## 2. BREAKING CHANGE HAZARDS AND MITIGATIONS
1. **Interface Expansion**: Avoid adding unimplemented methods to public interfaces to protect external implementers; favor extension methods over `IEventPublisher`.
2. **Record Properties**: Future additions to `EventEnvelope<T>` must use `init` accessors with default values (e.g., `EventMetadata.Empty`) to preserve source compatibility.