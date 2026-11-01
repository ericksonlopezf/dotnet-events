```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3


```
| Method                           | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean     | Error    | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |---------- |---------- |--------------- |------------ |------------ |---------:|---------:|--------:|------:|--------:|-------:|----------:|------------:|
| EventBus_Sequential_1Handler     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 404.6 ns |  0.99 ns | 0.92 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_5Handlers    | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 402.2 ns |  3.52 ns | 3.29 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Parallel_5Handlers      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 551.2 ns |  3.51 ns | 3.28 ns |     ? |       ? | 0.0439 |     744 B |           ? |
| EventBus_WithMiddleware_1Handler | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 489.7 ns |  1.67 ns | 1.48 ns |     ? |       ? | 0.0334 |     568 B |           ? |
| EventBus_Sequential_1Handler     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_5Handlers    | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Parallel_5Handlers      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_WithMiddleware_1Handler | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_1Handler     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_5Handlers    | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Parallel_5Handlers      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_WithMiddleware_1Handler | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
| EventBus_Sequential_1Handler     | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 390.1 ns | 15.11 ns | 0.83 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Sequential_5Handlers    | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 384.9 ns | 34.68 ns | 1.90 ns |     ? |       ? | 0.0339 |     568 B |           ? |
| EventBus_Parallel_5Handlers      | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 567.5 ns | 14.38 ns | 0.79 ns |     ? |       ? | 0.0439 |     744 B |           ? |
| EventBus_WithMiddleware_1Handler | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 455.3 ns | 16.24 ns | 0.89 ns |     ? |       ? | 0.0339 |     568 B |           ? |

Benchmarks with issues:
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
