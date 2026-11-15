```

BenchmarkDotNet v0.15.8, Linux Ubuntu 26.04.1 LTS (Resolute Raccoon)
AMD EPYC 9V45 4.32GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.404
  [Host]    : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v4


```
| Method                                     | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------------------------- |---------- |---------- |--------------- |------------ |------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| StaticRegistry_GetDescriptor_Cached        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  5.308 ns | 0.1428 ns | 0.1193 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N1        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  2.034 ns | 0.0498 ns | 0.0441 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N10       | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  2.412 ns | 0.0578 ns | 0.0512 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N100      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  2.016 ns | 0.0545 ns | 0.0483 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByEventType_N100 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 16.666 ns | 0.2453 ns | 0.2174 ns |     ? |       ? |         - |           ? |
| StaticRegistry_GetDescriptor_Cached        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| Registry_TryGetDescriptor_ByType_N1        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| Registry_TryGetDescriptor_ByType_N10       | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| Registry_TryGetDescriptor_ByType_N100      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| Registry_TryGetDescriptor_ByEventType_N100 | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| StaticRegistry_GetDescriptor_Cached        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| Registry_TryGetDescriptor_ByType_N1        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| Registry_TryGetDescriptor_ByType_N10       | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| Registry_TryGetDescriptor_ByType_N100      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| Registry_TryGetDescriptor_ByEventType_N100 | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
| StaticRegistry_GetDescriptor_Cached        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  5.602 ns | 3.8420 ns | 0.2106 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N1        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  2.068 ns | 0.4892 ns | 0.0268 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N10       | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  2.101 ns | 0.7473 ns | 0.0410 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N100      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  2.011 ns | 0.7885 ns | 0.0432 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByEventType_N100 | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 16.791 ns | 1.4457 ns | 0.0792 ns |     ? |       ? |         - |           ? |

Benchmarks with issues:
  RegistryBenchmarks.StaticRegistry_GetDescriptor_Cached: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  RegistryBenchmarks.Registry_TryGetDescriptor_ByType_N1: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  RegistryBenchmarks.Registry_TryGetDescriptor_ByType_N10: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  RegistryBenchmarks.Registry_TryGetDescriptor_ByType_N100: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  RegistryBenchmarks.Registry_TryGetDescriptor_ByEventType_N100: .NET 8.0(Runtime=.NET 8.0, Toolchain=net8.0)
  RegistryBenchmarks.StaticRegistry_GetDescriptor_Cached: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  RegistryBenchmarks.Registry_TryGetDescriptor_ByType_N1: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  RegistryBenchmarks.Registry_TryGetDescriptor_ByType_N10: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  RegistryBenchmarks.Registry_TryGetDescriptor_ByType_N100: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
  RegistryBenchmarks.Registry_TryGetDescriptor_ByEventType_N100: .NET 9.0(Runtime=.NET 9.0, Toolchain=net9.0)
