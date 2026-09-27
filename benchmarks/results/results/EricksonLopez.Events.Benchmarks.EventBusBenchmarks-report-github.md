```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.69GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


```
| Method                           | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean     | Error    | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------- |---------- |--------------- |------------ |------------ |---------:|---------:|--------:|------:|--------:|-------:|----------:|------------:|
| EventBus_Sequential_1Handler     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 437.6 ns |  1.94 ns | 1.82 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_5Handlers    | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 408.5 ns |  4.72 ns | 4.42 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Parallel_5Handlers      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 532.1 ns |  5.48 ns | 5.12 ns |     ? |       ? | 0.0439 |     744 B |           ? |
| EventBus_WithMiddleware_1Handler | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 468.6 ns |  2.24 ns | 2.10 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_1Handler     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_5Handlers    | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Parallel_5Handlers      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_WithMiddleware_1Handler | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_1Handler     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_5Handlers    | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Parallel_5Handlers      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_WithMiddleware_1Handler | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_1Handler     | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 436.6 ns | 66.68 ns | 3.66 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_5Handlers    | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 393.0 ns | 23.95 ns | 1.31 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Parallel_5Handlers      | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 533.0 ns | 32.79 ns | 1.80 ns |     ? |       ? | 0.0439 |     744 B |           ? |
| EventBus_WithMiddleware_1Handler | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 461.2 ns | 22.45 ns | 1.23 ns |     ? |       ? | 0.0339 |     568 B |           ? |

Benchmarks with issues:
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
