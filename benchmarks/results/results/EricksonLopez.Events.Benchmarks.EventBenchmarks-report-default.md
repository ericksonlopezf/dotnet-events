
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4


 Method                      | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error         | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
---------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|--------------:|-----------:|------:|--------:|-------:|----------:|------------:|
 EventId_New                 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   503.565 ns |     8.9826 ns |  8.4023 ns |     ? |       ? |      - |         - |           ? |
 EventId_TryFormat_ZeroAlloc | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     1.558 ns |     0.0181 ns |  0.0160 ns |     ? |       ? |      - |         - |           ? |
 Envelope_Create             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    19.735 ns |     0.4366 ns |  0.5028 ns |     ? |       ? | 0.0048 |      80 B |           ? |
 Envelope_Serialize_Json     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   568.705 ns |     5.8647 ns |  5.1989 ns |     ? |       ? | 0.0610 |    1048 B |           ? |
 Envelope_Deserialize_Json   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 1,376.806 ns |    27.1949 ns | 39.8619 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
 Event_Publish_InMemory      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   216.934 ns |     4.2886 ns |  7.5111 ns |     ? |       ? | 0.0372 |     624 B |           ? |
 EventId_New                 | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 EventId_TryFormat_ZeroAlloc | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Create             | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Serialize_Json     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Deserialize_Json   | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Event_Publish_InMemory      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 EventId_New                 | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 EventId_TryFormat_ZeroAlloc | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Create             | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Serialize_Json     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Deserialize_Json   | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Event_Publish_InMemory      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |            NA |         NA |     ? |       ? |     NA |        NA |           ? |
 EventId_New                 | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   489.253 ns |   249.1271 ns | 13.6555 ns |     ? |       ? |      - |         - |           ? |
 EventId_TryFormat_ZeroAlloc | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     1.569 ns |     0.2189 ns |  0.0120 ns |     ? |       ? |      - |         - |           ? |
 Envelope_Create             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    21.065 ns |    11.4228 ns |  0.6261 ns |     ? |       ? | 0.0048 |      80 B |           ? |
 Envelope_Serialize_Json     | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   568.074 ns |    61.2547 ns |  3.3576 ns |     ? |       ? | 0.0620 |    1048 B |           ? |
 Envelope_Deserialize_Json   | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 1,447.765 ns | 1,357.1433 ns | 74.3896 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
 Event_Publish_InMemory      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   211.584 ns |   134.2065 ns |  7.3563 ns |     ? |       ? | 0.0372 |     624 B |           ? |

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
