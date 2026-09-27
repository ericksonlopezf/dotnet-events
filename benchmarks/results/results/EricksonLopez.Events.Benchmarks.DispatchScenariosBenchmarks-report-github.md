```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.69GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


```
| Method                        | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean         | Error        | StdDev      | Ratio | RatioSD | Gen0    | Allocated | Alloc Ratio |
|------------------------------ |---------- |---------- |--------------- |------------ |------------ |-------------:|-------------:|------------:|------:|--------:|--------:|----------:|------------:|
| Publish_1Handler              | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     381.7 ns |      2.32 ns |     2.17 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_5Handlers             | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |     705.4 ns |      2.35 ns |     2.08 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_10Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   1,115.3 ns |      3.37 ns |     3.15 ns |     ? |       ? |  0.0362 |     624 B |           ? |
| Publish_50Handlers            | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |   4,398.4 ns |      4.89 ns |     3.82 ns |     ? |       ? |  0.0305 |     624 B |           ? |
| Publish_100Events_Sequential  | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  36,618.4 ns |    292.78 ns |   259.55 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
| Publish_1000Events_Sequential | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 358,777.3 ns |  1,818.16 ns | 1,611.75 ns |     ? |       ? | 37.1094 |  624009 B |           ? |
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
| Publish_1Handler              | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     379.6 ns |     12.56 ns |     0.69 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_5Handlers             | ShortRun  | .NET 10.0 | 3              | 1           | 3           |     689.3 ns |     78.71 ns |     4.31 ns |     ? |       ? |  0.0372 |     624 B |           ? |
| Publish_10Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   1,098.6 ns |     27.51 ns |     1.51 ns |     ? |       ? |  0.0362 |     624 B |           ? |
| Publish_50Handlers            | ShortRun  | .NET 10.0 | 3              | 1           | 3           |   4,407.6 ns |    132.93 ns |     7.29 ns |     ? |       ? |  0.0305 |     624 B |           ? |
| Publish_100Events_Sequential  | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  35,335.4 ns |  2,519.67 ns |   138.11 ns |     ? |       ? |  3.7231 |   62401 B |           ? |
| Publish_1000Events_Sequential | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 362,392.0 ns | 49,581.03 ns | 2,717.70 ns |     ? |       ? | 37.1094 |  624009 B |           ? |

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
