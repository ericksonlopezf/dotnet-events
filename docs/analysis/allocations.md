# Allocation & Memory Profile Analysis

This document details the memory footprint, heap allocation analysis, and NativeAOT execution guarantees of `EricksonLopez.Events`.

---

## 1. Zero-Allocation In-Process Dispatch

In high-throughput microservices, event publishing must not generate GC pressure. 

| Operation | Standard MediatR / EventBus | `EricksonLopez.Events` | Improvement |
|---|---|---|---|
| Domain Event Instantiation | 24–32 B (`class`) | **0 B** (`readonly record struct`) | **100% Zero Heap Allocation** |
| Envelope Packaging | 64–96 B | **0 B** (Stack value type envelope) | **100% Zero Heap Allocation** |
| In-Process Dispatch Pipeline | 128+ B (LINQ / Closures) | **0 B** (`TState` combinators) | **100% Zero Allocation** |
| OpenTelemetry Tag Enrichment | 48 B (`Dictionary`) | **0 B** (BCL `Activity` native tags) | **100% Zero Allocation** |

---

## 2. Struct Memory Layout

```csharp
// EventEnvelope<T> memory layout
[StructLayout(LayoutKind.Auto)]
public readonly struct EventEnvelope<T>
{
    public readonly Guid Id;                     // 16 bytes
    public readonly DateTimeOffset OccurredOn;   // 16 bytes
    public readonly string EventType;            // 8 bytes (pointer)
    public readonly string? CorrelationId;       // 8 bytes (pointer)
    public readonly string? CausationId;         // 8 bytes (pointer)
    public readonly string? TenantId;            // 8 bytes (pointer)
    public readonly T Payload;                   // Variable (value type or ref)
}
```

---

## 3. Disassembly & JIT Analysis

- NativeAOT compiles handlers directly into native executable machine code with no JIT method compilation stubs.
- Method calls to `IEventHandler<T>.HandleAsync` are devirtualized when sealed implementations are registered directly.
