
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4


 Method                      | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error       | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
---------------------------- |---------- |---------- |--------------- |------------ |------------ |-------------:|------------:|-----------:|------:|--------:|-------:|----------:|------------:|
 EventId_New                 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   500.495 ns |   4.3541 ns |  3.6359 ns |     ? |       ? |      - |         - |           ? |
 EventId_TryFormat_ZeroAlloc | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     1.586 ns |   0.0281 ns |  0.0235 ns |     ? |       ? |      - |         - |           ? |
 Envelope_Create             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |    19.691 ns |   0.4357 ns |  0.6387 ns |     ? |       ? | 0.0048 |      80 B |           ? |
 Envelope_Serialize_Json     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   561.660 ns |   2.3330 ns |  1.8215 ns |     ? |       ? | 0.0610 |    1048 B |           ? |
 Envelope_Deserialize_Json   | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 1,344.676 ns |  12.6446 ns | 11.8278 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
 Event_Publish_InMemory      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   198.986 ns |   1.8198 ns |  1.6132 ns |     ? |       ? | 0.0372 |     624 B |           ? |
 EventId_New                 | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 EventId_TryFormat_ZeroAlloc | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Create             | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Serialize_Json     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Deserialize_Json   | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Event_Publish_InMemory      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 EventId_New                 | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 EventId_TryFormat_ZeroAlloc | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Create             | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Serialize_Json     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Envelope_Deserialize_Json   | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 Event_Publish_InMemory      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |         NA |     ? |       ? |     NA |        NA |           ? |
 EventId_New                 | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   497.260 ns |  87.2992 ns |  4.7852 ns |     ? |       ? |      - |         - |           ? |
 EventId_TryFormat_ZeroAlloc | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     1.584 ns |   0.5689 ns |  0.0312 ns |     ? |       ? |      - |         - |           ? |
 Envelope_Create             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |    19.026 ns |   3.8024 ns |  0.2084 ns |     ? |       ? | 0.0048 |      80 B |           ? |
 Envelope_Serialize_Json     | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   546.422 ns | 106.1826 ns |  5.8202 ns |     ? |       ? | 0.0610 |    1048 B |           ? |
 Envelope_Deserialize_Json   | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 1,389.432 ns | 201.7795 ns | 11.0602 ns |     ? |       ? | 0.0992 |    1680 B |           ? |
 Event_Publish_InMemory      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   197.449 ns |  35.2986 ns |  1.9348 ns |     ? |       ? | 0.0372 |     624 B |           ? |

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
