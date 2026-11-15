```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V45 4.32GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


```
| Method                      | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error       | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|------------:|-----------:|------:|--------:|-------:|----------:|------------:|
| EventId_New                 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   510.630 ns |   4.6768 ns |  4.1459 ns |     ? |       ? |      - |         - |           ? |
| EventId_TryFormat_ZeroAlloc | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     1.738 ns |   0.0555 ns |  0.0594 ns |     ? |       ? |      - |         - |           ? |
| Envelope_Create             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    19.349 ns |   0.4290 ns |  0.6014 ns |     ? |       ? | 0.0048 |      80 B |           ? |
| Envelope_Serialize_Json     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   562.465 ns |   7.8913 ns |  6.9955 ns |     ? |       ? | 0.0620 |    1048 B |           ? |
| Envelope_Deserialize_Json   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 1,397.616 ns |  20.9106 ns | 18.5367 ns |     ? |       ? | 0.0916 |    1680 B |           ? |
| Event_Publish_InMemory      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   219.806 ns |   4.3699 ns |  6.1261 ns |     ? |       ? | 0.0372 |     624 B |           ? |
| EventId_New                 | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| EventId_TryFormat_ZeroAlloc | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Create             | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Serialize_Json     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Deserialize_Json   | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| Event_Publish_InMemory      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| EventId_New                 | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| EventId_TryFormat_ZeroAlloc | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Create             | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Serialize_Json     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| Envelope_Deserialize_Json   | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| Event_Publish_InMemory      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
| EventId_New                 | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   511.019 ns | 221.9627 ns | 12.1665 ns |     ? |       ? |      - |         - |           ? |
| EventId_TryFormat_ZeroAlloc | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     1.527 ns |   0.3641 ns |  0.0200 ns |     ? |       ? |      - |         - |           ? |
| Envelope_Create             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    18.833 ns |   2.8378 ns |  0.1555 ns |     ? |       ? | 0.0048 |      80 B |           ? |
| Envelope_Serialize_Json     | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   579.848 ns | 356.9798 ns | 19.5673 ns |     ? |       ? | 0.0620 |    1048 B |           ? |
| Envelope_Deserialize_Json   | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 1,431.271 ns | 713.9645 ns | 39.1348 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
| Event_Publish_InMemory      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   206.225 ns |  18.4143 ns |  1.0094 ns |     ? |       ? | 0.0372 |     624 B |           ? |

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
