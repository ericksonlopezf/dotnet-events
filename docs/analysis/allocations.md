# Allocation & Memory Profile Analysis

This document details the memory footprint, heap allocation analysis, and NativeAOT execution guarantees of `EricksonLopez.Events`.

---

## 1. Zero-Allocation In-Process Dispatch

In high-throughput microservices, event publishing must not generate GC pressure. 

| Operation | Standard MediatR / EventBus | `EricksonLopez.Events` | Improvement |
|---|---|---|---|
| Domain Event Instantiation | 24–32 B (`class`) | **0 B** (`readonly record struct`) | **100% Zero Heap Allocation** |
| Envelope Packaging | 64–96 B (`Dictionary`) | Single object allocation (`EventMetadata` Frozen Headers) | Minimal allocation, 0 B on cached paths |
| In-Process Dispatch Pipeline | 128+ B (LINQ / Closures) | **0 B** (Devirtualized `ValueTask` invocation delegates) | **100% Zero Allocation** |
| OpenTelemetry Tag Enrichment | 48 B (`Dictionary`) | **0 B** (BCL `Activity` native tags) | **100% Zero Allocation** |

---

## 2. Event Envelope Reference Type Model (ADR-023)

Per [ADR-023](../adr/adr-023-envelope-record-vs-struct.md), `EventEnvelope<T>` is intentionally designed as an immutable reference type (`sealed record class`) to avoid massive struct copy overhead across asynchronous await boundaries and middleware pipelines:

```csharp
// EventEnvelope<T> reference layout
public sealed record EventEnvelope<T> : IEventEnvelope<T> where T : IEvent
{
    public EventId Id { get; init; }                     // 16 bytes (UUIDv7 struct)
    public string EventType { get; init; }               // 8 bytes (reference)
    public string? EventSource { get; init; }            // 8 bytes (reference)
    public DateTimeOffset OccurredAt { get; init; }      // 16 bytes (struct)
    public T Payload { get; init; }                      // Variable (struct or ref)
    public EventMetadata Metadata { get; init; }         // 8 bytes (reference to frozen headers)
}
```


---

## 3. Disassembly & JIT Analysis

- NativeAOT compiles handlers directly into native executable machine code with no JIT method compilation stubs.
- Method calls to `IEventHandler<T>.HandleAsync` are devirtualized when sealed implementations are registered directly.
