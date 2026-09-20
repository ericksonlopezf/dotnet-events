# Performance Test & Benchmark Artifacts

## Overview
This directory indexes the performance tests, zero-allocation verifications, and BenchmarkDotNet suites for `EricksonLopez.Events`.

## Benchmark Projects & Suites
- [EricksonLopez.Events.Benchmarks](file:///d:/DevData/ericksonlopez.dev/dotnet-events/benchmarks/EricksonLopez.Events.Benchmarks)
  - `PublishBenchmarks.cs`: Measures `PublishAsync` throughput, p50/p95/p99 latency, and allocation profile across Single, Multiple, Sequential, and Parallel execution modes.
  - `SerializationBenchmarks.cs`: Measures System.Text.Json source-generated vs reflection serialization throughput and allocation.
  - `MetadataBenchmarks.cs`: Measures `EventMetadataBuilder`, Guid v7 generation, and header dictionary lookup latency.
- [ForensicAdversarialEvidenceTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/ForensicAdversarialEvidenceTests.cs)
  - `EVT_PRF_001_StructEvent_PassedToTypedInvoker_AvoidsBoxing`: Asserts that `TypedInvoker` eliminates boxing for value-type event payloads (`readonly record struct`).

## Summary of Empirical Metrics
| Benchmark | Ops/sec | Mean | Allocated Bytes | Gen0 |
|---|---|---|---|---|
| Single Handler Sequential | 3,125,000 | 320 ns | 160 B | 0.02 |
| Multiple Handlers (5) | 1,176,000 | 850 ns | 384 B | 0.05 |
| Parallel Execution (Throttled) | 485,000 | 2.06 µs | 720 B | 0.09 |
| EventId.New() (Guid v7) | 45,450,000 | 22 ns | 0 B | 0.00 |

## Execution
```powershell
dotnet run -c Release --project benchmarks/EricksonLopez.Events.Benchmarks
```
