
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V45 4.32GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


 Method                        | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error        | StdDev      | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
------------------------------ |---------- |---------- |--------------- |------------ |------------ |-------------:|-------------:|------------:|------:|--------:|--------:|----------:|------------:|
 Publish_1Handler              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     214.5 ns |      1.99 ns |     1.76 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_5Handlers             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     466.8 ns |      9.09 ns |    12.45 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_10Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     758.7 ns |      4.94 ns |     4.12 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_50Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   3,098.3 ns |     54.49 ns |    48.30 ns |     ? |       ? |  0.0343 |     624 B |           ? |
 Publish_100Events_Sequential  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  20,620.8 ns |    412.32 ns |   860.66 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
 Publish_1000Events_Sequential | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 204,014.4 ns |  4,060.71 ns | 6,201.13 ns |     ? |       ? | 37.1094 |  624009 B |           ? |
 Publish_1Handler              | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_5Handlers             | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_10Handlers            | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_50Handlers            | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_100Events_Sequential  | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_1000Events_Sequential | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_1Handler              | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_5Handlers             | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_10Handlers            | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_50Handlers            | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_100Events_Sequential  | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_1000Events_Sequential | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
 Publish_1Handler              | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     208.6 ns |     48.88 ns |     2.68 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_5Handlers             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     475.3 ns |    192.38 ns |    10.54 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_10Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     754.5 ns |     65.79 ns |     3.61 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_50Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   3,129.0 ns |    344.80 ns |    18.90 ns |     ? |       ? |  0.0343 |     624 B |           ? |
 Publish_100Events_Sequential  | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  20,851.7 ns |  8,246.73 ns |   452.03 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
 Publish_1000Events_Sequential | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 215,657.1 ns | 96,562.91 ns | 5,292.94 ns |     ? |       ? | 37.1094 |  624009 B |           ? |

Benchmarks with issues:
  DispatchScenariosBenchmarks.Publish_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  DispatchScenariosBenchmarks.Publish_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  DispatchScenariosBenchmarks.Publish_10Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  DispatchScenariosBenchmarks.Publish_50Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  DispatchScenariosBenchmarks.Publish_100Events_Sequential: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  DispatchScenariosBenchmarks.Publish_1000Events_Sequential: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  DispatchScenariosBenchmarks.Publish_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  DispatchScenariosBenchmarks.Publish_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  DispatchScenariosBenchmarks.Publish_10Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  DispatchScenariosBenchmarks.Publish_50Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  DispatchScenariosBenchmarks.Publish_100Events_Sequential: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  DispatchScenariosBenchmarks.Publish_1000Events_Sequential: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
