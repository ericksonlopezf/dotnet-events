
BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.5 LTS (Noble Numbat)
AMD EPYC 7763 2.69GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK 10.0.401
  [Host]    : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  ShortRun  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3


 Method                                     | Job       | Runtime   | IterationCount | LaunchCount | WarmupCount | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
------------------------------------------- |---------- |---------- |--------------- |------------ |------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
 StaticRegistry_GetDescriptor_Cached        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  9.007 ns | 0.0070 ns | 0.0062 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N1        | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.448 ns | 0.0100 ns | 0.0093 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N10       | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.445 ns | 0.0153 ns | 0.0135 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N100      | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     |  4.433 ns | 0.0085 ns | 0.0071 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByEventType_N100 | .NET 10.0 | .NET 10.0 | Default        | Default     | Default     | 25.589 ns | 0.0304 ns | 0.0270 ns |     ? |       ? |         - |           ? |
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
 StaticRegistry_GetDescriptor_Cached        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  8.902 ns | 0.3216 ns | 0.0176 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N1        | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  4.003 ns | 0.2001 ns | 0.0110 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N10       | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  3.977 ns | 0.1159 ns | 0.0064 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByType_N100      | ShortRun  | .NET 10.0 | 3              | 1           | 3           |  3.977 ns | 0.0456 ns | 0.0025 ns |     ? |       ? |         - |           ? |
 Registry_TryGetDescriptor_ByEventType_N100 | ShortRun  | .NET 10.0 | 3              | 1           | 3           | 26.380 ns | 0.4802 ns | 0.0263 ns |     ? |       ? |         - |           ? |

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
