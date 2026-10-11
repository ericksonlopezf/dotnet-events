```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


```
| Method                        | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error        | StdDev      | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
|------------------------------ |---------- |---------- |--------------- |------------ |------------ |-------------:|-------------:|------------:|------:|--------:|--------:|----------:|------------:|
| Publish_1Handler              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     363.6 ns |      1.87 ns |     1.75 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_5Handlers             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     691.3 ns |      3.73 ns |     3.31 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_10Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   1,109.6 ns |      2.70 ns |     2.52 ns |     ? |       ? |  0.0362 |     624 B |           ? |
| Publish_50Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   4,591.2 ns |      9.54 ns |     8.45 ns |     ? |       ? |  0.0305 |     624 B |           ? |
| Publish_100Events_Sequential  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  34,529.8 ns |    191.85 ns |   179.46 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
| Publish_1000Events_Sequential | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 341,030.0 ns |  1,622.70 ns | 1,438.48 ns |     ? |       ? | 37.1094 |  624009 B |           ? |
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
| Publish_1Handler              | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     356.3 ns |     37.01 ns |     2.03 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_5Handlers             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     690.7 ns |     42.24 ns |     2.32 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_10Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   1,118.9 ns |    137.70 ns |     7.55 ns |     ? |       ? |  0.0362 |     624 B |           ? |
| Publish_50Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   4,420.4 ns |    208.26 ns |    11.42 ns |     ? |       ? |  0.0305 |     624 B |           ? |
| Publish_100Events_Sequential  | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  40,776.2 ns |  3,300.18 ns |   180.89 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
| Publish_1000Events_Sequential | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 351,722.3 ns | 76,997.09 ns | 4,220.47 ns |     ? |       ? | 37.1094 |  624009 B |           ? |

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
