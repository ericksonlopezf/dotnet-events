
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4


 Method                        | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error        | StdDev      | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
------------------------------ |---------- |---------- |--------------- |------------ |------------ |-------------:|-------------:|------------:|------:|--------:|--------:|----------:|------------:|
 Publish_1Handler              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     262.2 ns |      0.65 ns |     0.61 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_5Handlers             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     540.2 ns |      0.72 ns |     0.64 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_10Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     887.6 ns |      1.19 ns |     1.06 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_50Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   3,654.2 ns |      1.90 ns |     1.59 ns |     ? |       ? |  0.0343 |     624 B |           ? |
 Publish_100Events_Sequential  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  26,517.7 ns |     44.38 ns |    37.06 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
 Publish_1000Events_Sequential | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 270,662.5 ns |  1,050.44 ns |   931.19 ns |     ? |       ? | 37.1094 |  624009 B |           ? |
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
 Publish_1Handler              | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     264.0 ns |     14.77 ns |     0.81 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_5Handlers             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     538.1 ns |      7.37 ns |     0.40 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_10Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     884.2 ns |     29.07 ns |     1.59 ns |     ? |       ? |  0.0372 |     624 B |           ? |
 Publish_50Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   3,658.7 ns |     50.17 ns |     2.75 ns |     ? |       ? |  0.0343 |     624 B |           ? |
 Publish_100Events_Sequential  | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  26,833.4 ns |    821.74 ns |    45.04 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
 Publish_1000Events_Sequential | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 264,008.9 ns | 33,383.09 ns | 1,829.84 ns |     ? |       ? | 37.1094 |  624009 B |           ? |

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
