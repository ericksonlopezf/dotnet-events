
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 9V45 2.60GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.402
  [Host]    : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  .NET 10.0 : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4
  ShortRun  : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v4


 Method                                     | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
------------------------------------------- |---------- |---------- |--------------- |------------ |------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
 StaticRegistry_GetDescriptor_Cached        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  5.490 ns | 0.1495 ns | 0.1325 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N1        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  2.060 ns | 0.0404 ns | 0.0378 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N10       | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  2.007 ns | 0.0425 ns | 0.0377 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N100      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  2.004 ns | 0.0574 ns | 0.0537 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByEventType_N100 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 16.779 ns | 0.3440 ns | 0.3532 ns |     ? |       ? |         - |           ? |
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
 StaticRegistry_GetDescriptor_Cached        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  6.066 ns | 3.7739 ns | 0.2069 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N1        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  2.059 ns | 1.3344 ns | 0.0731 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N10       | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  2.360 ns | 0.5683 ns | 0.0312 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N100      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  1.993 ns | 0.0506 ns | 0.0028 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByEventType_N100 | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 16.801 ns | 0.1633 ns | 0.0090 ns |     ? |       ? |         - |           ? |

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
