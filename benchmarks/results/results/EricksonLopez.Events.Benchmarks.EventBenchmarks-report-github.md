```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.69GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


```
| Method                      | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error       | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|------------:|----------:|------:|--------:|-------:|----------:|------------:|
| EventId_New                 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   689.862 ns |   0.9167 ns | 0.7654 ns |     ? |       ? |      - |         - |           ? |
| EventId_TryFormat_ZeroAlloc | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     3.253 ns |   0.0076 ns | 0.0071 ns |     ? |       ? |      - |         - |           ? |
| Envelope_Create             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    32.982 ns |   0.7116 ns | 0.8195 ns |     ? |       ? | 0.0048 |      80 B |           ? |
| Envelope_Serialize_Json     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 1,000.638 ns |   8.4102 ns | 7.4554 ns |     ? |       ? | 0.0610 |    1048 B |           ? |
| Envelope_Deserialize_Json   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 2,459.060 ns |   6.4657 ns | 5.3991 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
| Event_Publish_InMemory      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   361.705 ns |   3.7523 ns | 3.5099 ns |     ? |       ? | 0.0372 |     624 B |           ? |
| EventId_New                 | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EventId_TryFormat_ZeroAlloc | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Create             | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Serialize_Json     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Deserialize_Json   | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| Event_Publish_InMemory      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EventId_New                 | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EventId_TryFormat_ZeroAlloc | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Create             | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Serialize_Json     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Deserialize_Json   | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| Event_Publish_InMemory      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |        NA |     ? |       ? |     NA |        NA |           ? |
| EventId_New                 | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   689.123 ns |  79.4729 ns | 4.3562 ns |     ? |       ? |      - |         - |           ? |
| EventId_TryFormat_ZeroAlloc | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     3.364 ns |   0.1307 ns | 0.0072 ns |     ? |       ? |      - |         - |           ? |
| Envelope_Create             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    33.605 ns |   5.1211 ns | 0.2807 ns |     ? |       ? | 0.0048 |      80 B |           ? |
| Envelope_Serialize_Json     | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   988.877 ns | 110.2562 ns | 6.0435 ns |     ? |       ? | 0.0610 |    1048 B |           ? |
| Envelope_Deserialize_Json   | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 2,508.555 ns |  88.7889 ns | 4.8668 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
| Event_Publish_InMemory      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   359.577 ns |  41.7194 ns | 2.2868 ns |     ? |       ? | 0.0372 |     624 B |           ? |

Benchmarks with issues:
  EventBenchmarks.EventId_New: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBenchmarks.EventId_TryFormat_ZeroAlloc: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBenchmarks.Envelope_Create: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBenchmarks.Envelope_Serialize_Json: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBenchmarks.Envelope_Deserialize_Json: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBenchmarks.Event_Publish_InMemory: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBenchmarks.EventId_New: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBenchmarks.EventId_TryFormat_ZeroAlloc: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBenchmarks.Envelope_Create: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBenchmarks.Envelope_Serialize_Json: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBenchmarks.Envelope_Deserialize_Json: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBenchmarks.Event_Publish_InMemory: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
