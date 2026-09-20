# 34. BENCHMARKDOTNET TECHNICAL REPORT

## 1. ENVIRONMENT SPECIFICATIONS
- **BenchmarkDotNet**: v0.15.8
- **Processor**: AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 8 physical cores, 8 logical cores.
- **RAM**: 32 GB DDR5
- **Operating System**: Windows 11 Enterprise x64 (Build 26200.9168)
- **Compiler / SDK**: .NET SDK 10.0.400 / .NET 10.0.11 RyuJIT x86-64-v4

---

## 2. DETAILED BENCHMARK RESULTS WITH ALLOCATION PROFILING

| Benchmark | Mean | StdDev | Gen0 | Gen1 | Gen2 | Allocation |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| `EventId_New` | 59.028 ns | 0.679 ns | - | - | - | **0 B** |
| `EventId_TryFormat_ZeroAlloc` | 1.461 ns | 0.008 ns | - | - | - | **0 B** |
| `Envelope_Create` | 10.879 ns | 0.058 ns | 0.0016 | - | - | **80 B** |
| `Envelope_Serialize_Json` | 465.480 ns | 2.752 ns | 0.0267 | - | - | **1,360 B** |
| `Envelope_Deserialize_Json` | 1,073.140 ns | 3.393 ns | 0.0401 | - | - | **2,088 B** |
| `Event_Publish_InMemory` | 87.931 ns | 0.247 ns | 0.0062 | - | - | **312 B** |
| `StaticRegistry_GetDescriptor_Cached` | 5.695 ns | 0.040 ns | - | - | - | **0 B** |
| `Registry_TryGetDescriptor_ByType_N100` | 1.867 ns | 0.014 ns | - | - | - | **0 B** |
| `Registry_TryGetDescriptor_ByEventType_N100` | 13.669 ns | 0.031 ns | - | - | - | **0 B** |