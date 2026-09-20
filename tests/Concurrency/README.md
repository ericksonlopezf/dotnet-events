# Concurrency Test Artifacts & Regression Suite

## Overview
This directory indexes adversarial concurrent stress tests, thread-safety verifications, race condition probes, and transactional publisher synchronization tests for `EricksonLopez.Events`.

## Test Suites & Locations
- [ConcurrencyAndThreadSafetyTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Concurrency/ConcurrencyAndThreadSafetyTests.cs)
  - `EventBus_ConcurrentPublishers_NoRaceConditions`
  - `HandlerRegistry_ConcurrentRegistrationAndRead_ThreadSafe`
  - `EventMetadataBuilder_ConcurrentBuild_ProducesDistinctInstances`
- [Stress10kConcurrentPublishersTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Concurrency/Stress10kConcurrentPublishersTests.cs)
  - `StressTest_10000ConcurrentEvents_ZeroLostUpdates`
  - `StressTest_100Publishers_100EventsEach_BarrierSynchronized`
  - `StressTest_SequentialAndParallel_UnderHeavyContention`
- [ParallelStrategyConcurrencyTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Concurrency/ParallelStrategyConcurrencyTests.cs)
  - `ParallelExecutionStrategy_MaxDegreeOfParallelism_IsStrictlyRespected`
  - `ParallelExecutionStrategy_TaskSchedulerStarvation_Prevented`
- [ForensicAdversarialEvidenceTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/ForensicAdversarialEvidenceTests.cs)
  - `EVT_CONC_002_TransactionalEventPublisher_ConcurrentCommit_IsThreadSafeAndLossless`

## Execution
```powershell
dotnet test tests/EricksonLopez.Events.UnitTests --filter "FullyQualifiedName~Concurrency"
```
