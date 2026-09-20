# Performance Benchmarks & Allocation Report

---

## 1. Event Dispatch Benchmarks (BenchmarkDotNet)

Benchmarks executed on `.NET 10.0.x` and `.NET 8.0.x` under Linux-x64 runner:

| Method | Framework | Mean | Error | StdDev | Gen0 | Allocated |
|---|---|---|---|---|---|---|
| `Publish_DomainEvent_InProcess` | .NET 10.0 | **12.4 ns** | 0.12 ns | 0.10 ns | - | **0 B** |
| `Publish_DomainEvent_InProcess` | .NET 8.0 | **14.8 ns** | 0.15 ns | 0.14 ns | - | **0 B** |
| `Envelope_Packaging_Create` | .NET 10.0 | **4.2 ns** | 0.05 ns | 0.04 ns | - | **0 B** |
| `CloudEvents_Serialize_AOT` | .NET 10.0 | **84.3 ns** | 0.82 ns | 0.76 ns | 0.0038 | 64 B |
| `Outbox_Envelope_Serialize` | .NET 10.0 | **62.1 ns** | 0.61 ns | 0.58 ns | 0.0029 | 48 B |

---

## 2. Key Takeaways

- Immutable `EventEnvelope<T>` (`sealed record class`) packaging executes in single-digit nanoseconds with zero GC overhead on cached/in-process paths.

