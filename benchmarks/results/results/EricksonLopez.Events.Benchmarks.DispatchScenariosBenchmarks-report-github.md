```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 4.30GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4


```
| Method                        | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error       | StdDev      | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
|------------------------------ |---------- |---------- |--------------- |------------ |------------ |-------------:|------------:|------------:|------:|--------:|--------:|----------:|------------:|
| Publish_1Handler              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     203.9 ns |     3.93 ns |     3.48 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_5Handlers             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     454.0 ns |     9.06 ns |     8.47 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_10Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     751.4 ns |    13.17 ns |    11.68 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_50Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   3,173.2 ns |    54.18 ns |    50.68 ns |     ? |       ? |  0.0343 |     624 B |           ? |
| Publish_100Events_Sequential  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  20,230.6 ns |   149.82 ns |   116.97 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
| Publish_1000Events_Sequential | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 200,835.9 ns | 3,797.82 ns | 3,552.48 ns |     ? |       ? | 37.1094 |  624009 B |           ? |
| Publish_1Handler              | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_5Handlers             | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_10Handlers            | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_50Handlers            | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_100Events_Sequential  | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_1000Events_Sequential | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_1Handler              | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_5Handlers             | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_10Handlers            | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_50Handlers            | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_100Events_Sequential  | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_1000Events_Sequential | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |           NA |          NA |          NA |     ? |       ? |      NA |        NA |           ? |
| Publish_1Handler              | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     201.5 ns |    32.71 ns |     1.79 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_5Handlers             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     460.4 ns |   416.94 ns |    22.85 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_10Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     763.0 ns |    59.41 ns |     3.26 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_50Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   3,136.5 ns |   124.45 ns |     6.82 ns |     ? |       ? |  0.0343 |     624 B |           ? |
| Publish_100Events_Sequential  | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  20,369.5 ns | 3,622.25 ns |   198.55 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
| Publish_1000Events_Sequential | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 198,042.7 ns | 6,352.84 ns |   348.22 ns |     ? |       ? | 37.1094 |  624009 B |           ? |

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
