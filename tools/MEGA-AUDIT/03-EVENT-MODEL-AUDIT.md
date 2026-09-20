# 03. EVENT MODEL AUDIT

## 1. EVENT TAXONOMY

`EricksonLopez.Events.Contracts` defines a clean semantic hierarchy:
1. **`IEvent`**: Root marker interface requiring `EventId Id { get; }` and `DateTimeOffset OccurredAt { get; }`.
2. **`IDomainEvent`**: Subtype of `IEvent` representing domain occurrences within an Aggregate Root. Typically processed synchronously in the same process and transaction.
3. **`IIntegrationEvent`**: Subtype of `IEvent` representing events destined to cross Bounded Context boundaries. Designed to be serialized, transit via Outbox, and propagate business context.

---

## 2. `EventEnvelope<TEvent>` ANALYSIS

```csharp
public sealed record EventEnvelope<TEvent> : IEventEnvelope<TEvent>
    where TEvent : IEvent
{
    public required EventId Id { get; init; }
    public required EventType Type { get; init; }
    public required EventVersion Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required TEvent Payload { get; init; }
    public EventMetadata Metadata { get; init; } = EventMetadata.Empty;
}
```

### Envelope Strengths:
- **Payload Decoupling**: Transport and traceability metadata (`CorrelationId`, `CausationId`, `TenantId`, Headers) reside in the envelope and `EventMetadata`, keeping business event models clean of infrastructure concerns.
- **Explicit Versioning**: The `EventVersion` field (positive integer `1`, `2`, etc.) enables schema evolution, backward compatibility, and forward compatibility.
- **Memory Overhead**: An `EventEnvelope` instance allocates only **80 bytes** on the heap, with an instantiation latency of **10.88 ns** as measured in BenchmarkDotNet.

---

## 3. IMMUTABILITY AUDIT AND ROSLYN ANALYZER

The **`ELE001` (`EventImmutabilityAnalyzer`)** analyzer inspects the Roslyn syntax tree to ensure that any type implementing `IEvent`:
- Is declared as a `record` or `record struct`.
- Is not a standard mutable `class`.
- Has all properties declared with `init` accessors or as `readonly`.

### Adversarial Finding EVT-SEC-001 (Shallow vs Deep Immutability)
- **Vulnerability**: The C# compiler and analyzer enforce shallow immutability. If an event contains a mutable collection (e.g., `List<string> Items` or a nested object with public setters), a handler can mutate the list contents before subsequent handlers process the event.
- **Recommendation**: Guide consumers and provide Roslyn diagnostics warning against mutable collections (`List<T>`, `Dictionary<TKey, TValue>`) in favor of immutable collections (`IReadOnlyList<T>`, `ImmutableArray<T>`).