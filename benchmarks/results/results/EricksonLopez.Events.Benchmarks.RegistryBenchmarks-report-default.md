
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.45GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


 Method                                     | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
------------------------------------------- |---------- |---------- |--------------- |------------ |------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
 StaticRegistry_GetDescriptor_Cached        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  8.999 ns | 0.0135 ns | 0.0112 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N1        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.475 ns | 0.0157 ns | 0.0147 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N10       | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.466 ns | 0.0138 ns | 0.0129 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N100      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.468 ns | 0.0110 ns | 0.0103 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByEventType_N100 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 25.559 ns | 0.0248 ns | 0.0220 ns |     ? |       ? |         - |           ? |
 StaticRegistry_GetDescriptor_Cached        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 Registry_TryGetDescriptor_ByType_N1        | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 Registry_TryGetDescriptor_ByType_N10       | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 Registry_TryGetDescriptor_ByType_N100      | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 Registry_TryGetDescriptor_ByEventType_N100 | .NET 8.0  | .NET 8.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 StaticRegistry_GetDescriptor_Cached        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 Registry_TryGetDescriptor_ByType_N1        | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 Registry_TryGetDescriptor_ByType_N10       | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 Registry_TryGetDescriptor_ByType_N100      | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 Registry_TryGetDescriptor_ByEventType_N100 | .NET 9.0  | .NET 9.0  | Default        | Default     | Default     |        NA |        NA |        NA |     ? |       ? |        NA |           ? |
 StaticRegistry_GetDescriptor_Cached        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  9.036 ns | 4.7769 ns | 0.2618 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N1        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  4.606 ns | 1.2315 ns | 0.0675 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N10       | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  3.994 ns | 0.0661 ns | 0.0036 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N100      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  3.976 ns | 0.1362 ns | 0.0075 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByEventType_N100 | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25.530 ns | 1.4474 ns | 0.0793 ns |     ? |       ? |         - |           ? |

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
