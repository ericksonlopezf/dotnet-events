
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4


 Method                        | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error        | StdDev      | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
------------------------------ |---------- |---------- |--------------- |------------ |------------ |-------------:|-------------:|------------:|------:|--------:|--------:|----------:|------------:|
 Publish_1Handler              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     215.1 ns |      2.92 ns |     2.44 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_5Handlers             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     471.0 ns |      5.05 ns |     4.21 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_10Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     770.0 ns |      8.58 ns |     6.70 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_50Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   3,065.3 ns |     49.12 ns |    41.02 ns |     ? |       ? |  0.0343 |     624 B |           ? |
 Publish_100Events_Sequential  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  20,070.7 ns |    393.24 ns |   452.85 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
 Publish_1000Events_Sequential | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 207,035.1 ns |  4,102.15 ns | 5,883.18 ns |     ? |       ? | 37.1094 |  624009 B |           ? |
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
 Publish_1Handler              | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     209.6 ns |     91.50 ns |     5.02 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_5Handlers             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     451.7 ns |    322.34 ns |    17.67 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_10Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     764.4 ns |    533.01 ns |    29.22 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_50Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   3,161.4 ns |    531.18 ns |    29.12 ns |     ? |       ? |  0.0343 |     624 B |           ? |
 Publish_100Events_Sequential  | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  20,740.0 ns |  3,755.23 ns |   205.84 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
 Publish_1000Events_Sequential | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 205,690.5 ns | 65,631.37 ns | 3,597.48 ns |     ? |       ? | 37.1094 |  624009 B |           ? |

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
