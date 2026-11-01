```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.63GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.403
  [Host]    : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.14 (10.0.14, 10.0.1426.52326), X64 RyuJIT x86-64-v3


```
| Method                                     | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------------------------- |---------- |---------- |--------------- |------------ |------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| StaticRegistry_GetDescriptor_Cached        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  9.006 ns | 0.0142 ns | 0.0126 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N1        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.427 ns | 0.0091 ns | 0.0081 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N10       | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.800 ns | 0.0052 ns | 0.0040 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N100      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.430 ns | 0.0097 ns | 0.0086 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByEventType_N100 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 25.304 ns | 0.0130 ns | 0.0109 ns |     ? |       ? |         - |           ? |
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
| StaticRegistry_GetDescriptor_Cached        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  8.917 ns | 0.6293 ns | 0.0345 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N1        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  4.089 ns | 0.4967 ns | 0.0272 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N10       | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  3.992 ns | 0.1671 ns | 0.0092 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByType_N100      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  4.038 ns | 0.0219 ns | 0.0012 ns |     ? |       ? |         - |           ? |
| Registry_TryGetDescriptor_ByEventType_N100 | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 25.596 ns | 0.0598 ns | 0.0033 ns |     ? |       ? |         - |           ? |

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
