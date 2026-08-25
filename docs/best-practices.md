# Design Best Practices — EricksonLopez.Events

Rules, architectural patterns, and guidelines for designing clean, high-performance, event-driven systems.

---

## 1. Event Immutability

- Events represent **immutable facts that already occurred in the past**.
- Always define events as `sealed record` with read-only properties (`{ get; init; }` or positional parameters).
- The Roslyn code analyzer **ELE001** automatically validates at compile time that events are immutable.

## 2. Using Guid v7 Identifiers (`EventId`)

- Use `EventId.New()` instead of `Guid.NewGuid()`.
- GUID Version 7 contains a built-in 48-bit timestamp, enabling natural chronological ordering and eliminating B-Tree page fragmentation in databases.

## 3. Separation Between Domain Events and Integration Events

- **`IDomainEvent`**: Must remain in the `Domain` layer. Uses the Ubiquitous Language of the bounded context.
- **`IIntegrationEvent`**: Must reside in `Application` or a shared contract. Must not expose domain entities or aggregate types.
- The **ELE005** analyzer automatically detects if an integration event leaks internal domain event types.

## 4. Schema Evolution and Versioning

- Always decorate integration events with declarative attributes:
  ```csharp
  [EventName("billing.invoices.paid")]
  [EventVersion(1)]
  [EventSource("billing-service")]
  ```
- **Non-breaking rule**: If a backwards-incompatible change is required, increment the version with `[EventVersion(2)]` and create a new contract without breaking existing consumers of version 1.

## 5. Zero Reflection and AOT Compatibility

- Do not use dynamic invocations or `Assembly.GetTypes()` to scan handlers at runtime.
- Use the `EricksonLopez.Events.Generators` Source Generator or register explicitly via `AddEventHandler<TEvent, THandler>()`.
- Configure `JsonSerializerContext` for Native AOT serialization of all envelopes and payloads.
