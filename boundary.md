# Architectural Boundary Specification: EricksonLopez.Events.Contracts

## 1. Purpose
`EricksonLopez.Events.Contracts` provides pure, immutable domain and integration event contracts, event bus interfaces, and event metadata identifiers for decoupled, event-driven architecture.

## 2. Owns
- `IEvent`, `IDomainEvent`, `IIntegrationEvent`.
- `IEventBus`, `IEventPublisher`, `IEventSubscriber`, `IEventHandler<TEvent>`.
- `IEventEnvelope<TEvent>`, `EventMetadata`, `EventMetadataBuilder`.
- Identifiers: `EventId`, `EventType`, `EventVersion`, `CorrelationId`, `CausationId`, `TenantId` (event routing identity).

## 3. Does Not Own
- In-process event bus dispatch orchestration (`EricksonLopez.Events`).
- CloudEvents serialization formatting (`EricksonLopez.Events.CloudEvents`).
- Transactional outbox storage/relay (`EricksonLopez.Outbox` / `EricksonLopez.Events.Outbox`).
- Idempotent inbox message consumption (`EricksonLopez.Events.Inbox`).
- Tenancy resolution or catalog stores (`EricksonLopez.MultiTenancy`).

## 4. Allowed Dependencies
- **.NET BCL only**.
- **Zero** external or `EricksonLopez.*` dependencies.

## 5. Forbidden Dependencies
- `EricksonLopez.Result`, `EricksonLopez.SharedKernel`, `EricksonLopez.Mediator`.
- Message broker client SDKs (`RabbitMQ.Client`, `Confluent.Kafka`, `AWSSDK`).
- `Microsoft.Extensions.DependencyInjection` (confined to `EricksonLopez.Events`).

## 6. Who Can Depend On It
- `EricksonLopez.Events` (L3).
- `EricksonLopez.SharedKernel` (L1 - aggregate roots raising `IDomainEvent`).
- `EricksonLopez.Messaging.Events` (L3 - event transport adapter).
- `EricksonLopez.Processes.Events` (L3 - process manager event dispatcher).

## 7. Public API Rules
- All event marker interfaces and metadata records must be immutable.
- `TenantId` and `CorrelationId` represent event routing context identity only (see ADR-011).

## 8. AOT Expectations
- `IsAotCompatible=true`.

## 9. Trimming Expectations
- `IsTrimmable=true`.

## 10. Provider Isolation
- 100% transport- and broker-agnostic.

## 11. Testing Isolation
- InMemory bus fakes and event listener test doubles live in `EricksonLopez.Events.Testing`.
