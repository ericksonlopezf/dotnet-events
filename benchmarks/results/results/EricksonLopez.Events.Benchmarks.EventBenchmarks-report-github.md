```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4


```
| Method                      | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error       | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|------------:|-----------:|------:|--------:|-------:|----------:|------------:|
| EventId_New                 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   749.787 ns |   1.0224 ns |  0.9063 ns |     ? |       ? |      - |         - |           ? |
| EventId_TryFormat_ZeroAlloc | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     3.109 ns |   0.0079 ns |  0.0074 ns |     ? |       ? |      - |         - |           ? |
| Envelope_Create             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    25.326 ns |   0.1947 ns |  0.1821 ns |     ? |       ? | 0.0048 |      80 B |           ? |
| Envelope_Serialize_Json     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   665.468 ns |   6.1340 ns |  5.4377 ns |     ? |       ? | 0.0610 |    1048 B |           ? |
| Envelope_Deserialize_Json   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 1,773.208 ns |   2.9207 ns |  2.5891 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
| Event_Publish_InMemory      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   262.600 ns |   0.9536 ns |  0.8454 ns |     ? |       ? | 0.0372 |     624 B |           ? |
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
| EventId_New                 | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   733.299 ns |   4.0483 ns |  0.2219 ns |     ? |       ? |      - |         - |           ? |
| EventId_TryFormat_ZeroAlloc | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     2.753 ns |   0.1112 ns |  0.0061 ns |     ? |       ? |      - |         - |           ? |
| Envelope_Create             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    26.401 ns |   2.8297 ns |  0.1551 ns |     ? |       ? | 0.0048 |      80 B |           ? |
| Envelope_Serialize_Json     | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   673.541 ns | 191.1346 ns | 10.4767 ns |     ? |       ? | 0.0610 |    1048 B |           ? |
| Envelope_Deserialize_Json   | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 1,805.961 ns |  42.7230 ns |  2.3418 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
| Event_Publish_InMemory      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   260.327 ns |  20.6866 ns |  1.1339 ns |     ? |       ? | 0.0372 |     624 B |           ? |

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
