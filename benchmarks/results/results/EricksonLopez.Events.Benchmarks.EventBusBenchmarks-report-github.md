```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4


```
| Method                           | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean     | Error     | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------- |---------- |--------------- |------------ |------------ |---------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| EventBus_Sequential_1Handler     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 208.6 ns |   4.18 ns |  4.47 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_5Handlers    | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 214.0 ns |   4.07 ns |  5.30 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Parallel_5Handlers      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 261.1 ns |   3.89 ns |  4.16 ns |     ? |       ? | 0.0443 |     744 B |           ? |
| EventBus_WithMiddleware_1Handler | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 225.8 ns |   2.28 ns |  2.02 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_1Handler     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |        NA |       NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_5Handlers    | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |        NA |       NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Parallel_5Handlers      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |        NA |       NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_WithMiddleware_1Handler | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |        NA |       NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_1Handler     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |        NA |       NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_5Handlers    | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |        NA |       NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Parallel_5Handlers      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |        NA |       NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_WithMiddleware_1Handler | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |        NA |       NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_1Handler     | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 217.4 ns |  25.37 ns |  1.39 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_5Handlers    | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 205.1 ns |   9.35 ns |  0.51 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Parallel_5Handlers      | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 264.7 ns | 357.63 ns | 19.60 ns |     ? |       ? | 0.0443 |     744 B |           ? |
| EventBus_WithMiddleware_1Handler | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 229.3 ns |  81.48 ns |  4.47 ns |     ? |       ? | 0.0339 |     568 B |           ? |

Benchmarks with issues:
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
