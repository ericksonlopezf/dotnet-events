
BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V74 3.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v4


 Method                           | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean     | Error    | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
--------------------------------- |---------- |---------- |--------------- |------------ |------------ |---------:|---------:|--------:|------:|--------:|-------:|----------:|------------:|
 EventBus_Sequential_1Handler     | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 285.5 ns |  1.05 ns | 0.98 ns |     ? |       ? | 0.0339 |     568 B |           ? |
 EventBus_Sequential_5Handlers    | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 285.4 ns |  0.70 ns | 0.62 ns |     ? |       ? | 0.0339 |     568 B |           ? |
 EventBus_Parallel_5Handlers      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 343.0 ns |  1.14 ns | 1.07 ns |     ? |       ? | 0.0443 |     744 B |           ? |
 EventBus_WithMiddleware_1Handler | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 339.6 ns |  0.93 ns | 0.87 ns |     ? |       ? | 0.0339 |     568 B |           ? |
 EventBus_Sequential_1Handler     | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
 EventBus_Sequential_5Handlers    | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
 EventBus_Parallel_5Handlers      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
 EventBus_WithMiddleware_1Handler | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
 EventBus_Sequential_1Handler     | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
 EventBus_Sequential_5Handlers    | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
 EventBus_Parallel_5Handlers      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
 EventBus_WithMiddleware_1Handler | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |       NA |       NA |      NA |     ? |       ? |     NA |        NA |           ? |
 EventBus_Sequential_1Handler     | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 287.1 ns |  7.62 ns | 0.42 ns |     ? |       ? | 0.0339 |     568 B |           ? |
 EventBus_Sequential_5Handlers    | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 283.7 ns | 18.13 ns | 0.99 ns |     ? |       ? | 0.0339 |     568 B |           ? |
 EventBus_Parallel_5Handlers      | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 340.9 ns | 31.95 ns | 1.75 ns |     ? |       ? | 0.0443 |     744 B |           ? |
 EventBus_WithMiddleware_1Handler | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 322.8 ns | 31.22 ns | 1.71 ns |     ? |       ? | 0.0339 |     568 B |           ? |

Benchmarks with issues:
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  EventBusBenchmarks.EventBus_Sequential_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Sequential_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_Parallel_5Handlers: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  EventBusBenchmarks.EventBus_WithMiddleware_1Handler: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
