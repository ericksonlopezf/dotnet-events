```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V45 4.32GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


```
| Method                           | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean     | Error    | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------- |---------- |--------------- |------------ |------------ |---------:|---------:|--------:|------:|--------:|-------:|----------:|------------:|
| EventBus_Sequential_1Handler     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 206.7 ns |  4.03 ns | 6.28 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_5Handlers    | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 215.5 ns |  4.28 ns | 4.20 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Parallel_5Handlers      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 256.8 ns |  4.59 ns | 7.92 ns |     ? |       ? | 0.0443 |     744 B |           ? |
| EventBus_WithMiddleware_1Handler | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 240.4 ns |  4.79 ns | 5.70 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_1Handler     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_5Handlers    | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Parallel_5Handlers      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_WithMiddleware_1Handler | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_1Handler     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_5Handlers    | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Parallel_5Handlers      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_WithMiddleware_1Handler | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_1Handler     | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 224.6 ns | 55.27 ns | 3.03 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_5Handlers    | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 220.9 ns | 37.73 ns | 2.07 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Parallel_5Handlers      | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 267.1 ns | 69.98 ns | 3.84 ns |     ? |       ? | 0.0443 |     744 B |           ? |
| EventBus_WithMiddleware_1Handler | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 239.3 ns | 46.68 ns | 2.56 ns |     ? |       ? | 0.0339 |     568 B |           ? |

Benchmarks with issues:
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
