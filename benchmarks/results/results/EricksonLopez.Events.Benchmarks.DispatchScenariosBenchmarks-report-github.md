```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3


```
| Method                        | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error        | StdDev      | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
|------------------------------ |---------- |---------- |--------------- |------------ |------------ |-------------:|-------------:|------------:|------:|--------:|--------:|----------:|------------:|
| Publish_1Handler              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     358.9 ns |      4.62 ns |     4.32 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_5Handlers             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     700.1 ns |      2.99 ns |     2.79 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_10Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   1,101.3 ns |      3.17 ns |     2.81 ns |     ? |       ? |  0.0362 |     624 B |           ? |
| Publish_50Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   4,402.4 ns |      2.08 ns |     1.84 ns |     ? |       ? |  0.0305 |     624 B |           ? |
| Publish_100Events_Sequential  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  34,797.6 ns |    157.19 ns |   147.04 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
| Publish_1000Events_Sequential | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 370,971.3 ns |  1,104.02 ns |   978.69 ns |     ? |       ? | 37.1094 |  624009 B |           ? |
| Publish_1Handler              | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_5Handlers             | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_10Handlers            | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_50Handlers            | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_100Events_Sequential  | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_1000Events_Sequential | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_1Handler              | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_5Handlers             | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_10Handlers            | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_50Handlers            | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_100Events_Sequential  | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_1000Events_Sequential | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |           NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_1Handler              | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     365.6 ns |      7.57 ns |     0.42 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_5Handlers             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     702.4 ns |     35.77 ns |     1.96 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_10Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   1,104.0 ns |     36.82 ns |     2.02 ns |     ? |       ? |  0.0362 |     624 B |           ? |
| Publish_50Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   4,405.8 ns |    195.79 ns |    10.73 ns |     ? |       ? |  0.0305 |     624 B |           ? |
| Publish_100Events_Sequential  | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  36,727.3 ns |  5,552.96 ns |   304.38 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
| Publish_1000Events_Sequential | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 349,879.2 ns | 25,550.57 ns | 1,400.51 ns |     ? |       ? | 37.1094 |  624009 B |           ? |

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
