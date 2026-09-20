# 13. FORENSIC PERFORMANCE AUDIT (BENCHMARKS & LATENCY)

## 1. BENCHMARK ENVIRONMENT AND TEST CONDITIONS
- **Tool**: BenchmarkDotNet v0.15.8
- **Hardware**: AMD Ryzen 7 9800X3D 4.70GHz, 8 physical cores, 8 logical cores.
- **Operating System**: Windows 11 Enterprise (25H2 Build 26200.9168)
- **Runtime**: .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT x86-64-v4
- **Configuration**: ShortRunJob (Warmup=3, Iterations=3, Launch=1)

---

## 2. MEASURED EMPIRICAL RESULTS: CORE OPERATIONS

| Benchmark Method | Mean | Error | StdDev | Heap Allocation | Relative Ratio |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **`EventId_New`** (GUID v7 creation) | **59.03 ns** | 12.39 ns | 0.68 ns | **0 B** | Baseline (1.00) |
| **`EventId_TryFormat_ZeroAlloc`** | **1.46 ns** | 0.15 ns | 0.01 ns | **0 B** | **0.02x (Ultra-fast)** |
| **`Envelope_Create`** | **10.88 ns** | 1.05 ns | 0.06 ns | **80 B** | 0.18x |
| **`Event_Publish_InMemory`** | **87.93 ns** | 4.51 ns | 0.25 ns | **312 B** | 1.49x |
| **`Envelope_Serialize_Json`** | **465.48 ns** | 50.20 ns | 2.75 ns | **1,360 B** | 7.89x |
| **`Envelope_Deserialize_Json`** | **1,073.14 ns** | 61.90 ns | 3.39 ns | **2,088 B** | 18.18x |

---

## 3. MEASURED RESULTS: TYPE REGISTRATION AND RESOLUTION

| Benchmark Method | Mean | StdDev | Heap Allocation | Throughput (Ops/sec) |
| :--- | :---: | :---: | :---: | :---: |
| **`StaticRegistry_GetDescriptor_Cached`** | **5.695 ns** | 0.040 ns | **0 B** | **175,590,000 ops/s** |
| **`Registry_TryGetDescriptor_ByType_N1`** | **1.820 ns** | 0.013 ns | **0 B** | **549,450,000 ops/s** |
| **`Registry_TryGetDescriptor_ByType_N10`** | **1.859 ns** | 0.018 ns | **0 B** | **537,920,000 ops/s** |
| **`Registry_TryGetDescriptor_ByType_N100`**| **1.867 ns** | 0.014 ns | **0 B** | **535,610,000 ops/s** |
| **`Registry_TryGetDescriptor_ByEventType_N100`**| **13.669 ns** | 0.031 ns | **0 B** | **73,150,000 ops/s** |

### Forensic Performance Insights:
1. Lookup by CLR type in the registry is strictly $O(1)$, maintaining an invariant latency of **1.86 ns** whether the registry contains 1 or 100 registered event types.
2. Zero bytes allocated on the heap during type lookup and registration resolution.
3. In-memory event dispatch throughput exceeds **11.3 million dispatches per second per core**.