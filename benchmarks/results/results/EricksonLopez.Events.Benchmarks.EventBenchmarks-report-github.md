```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```
| Method                      | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error       | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|------------:|-----------:|------:|--------:|-------:|----------:|------------:|
| EventId_New                 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   491.979 ns |   9.7627 ns | 10.4460 ns |     ? |       ? |      - |         - |           ? |
| EventId_TryFormat_ZeroAlloc | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     1.613 ns |   0.0207 ns |  0.0183 ns |     ? |       ? |      - |         - |           ? |
| Envelope_Create             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    19.870 ns |   0.4346 ns |  0.3853 ns |     ? |       ? | 0.0048 |      80 B |           ? |
| Envelope_Serialize_Json     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   564.012 ns |   8.9813 ns |  7.9617 ns |     ? |       ? | 0.0620 |    1048 B |           ? |
| Envelope_Deserialize_Json   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 1,381.067 ns |  19.0594 ns | 15.9154 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
| Event_Publish_InMemory      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   205.713 ns |   2.5388 ns |  2.2506 ns |     ? |       ? | 0.0372 |     624 B |           ? |
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
| EventId_New                 | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   499.695 ns |   0.9773 ns |  0.0536 ns |     ? |       ? |      - |         - |           ? |
| EventId_TryFormat_ZeroAlloc | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     1.608 ns |   1.1099 ns |  0.0608 ns |     ? |       ? |      - |         - |           ? |
| Envelope_Create             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    22.116 ns |   4.0568 ns |  0.2224 ns |     ? |       ? | 0.0048 |      80 B |           ? |
| Envelope_Serialize_Json     | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   580.977 ns | 218.7221 ns | 11.9889 ns |     ? |       ? | 0.0610 |    1048 B |           ? |
| Envelope_Deserialize_Json   | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 1,476.528 ns | 846.7652 ns | 46.4141 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
| Event_Publish_InMemory      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   199.050 ns |  25.0127 ns |  1.3710 ns |     ? |       ? | 0.0372 |     624 B |           ? |

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
