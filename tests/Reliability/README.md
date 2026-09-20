# Reliability Test Artifacts & Regression Suite

## Overview
This directory indexes failure injection, reentrancy boundary defense, transaction boundary safety, and error escalation tests for `EricksonLopez.Events`.

## Test Suites & Locations
- [AdversarialChaosAndFailureInjectionTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Reliability/AdversarialChaosAndFailureInjectionTests.cs)
  - `FailureInjection_HandlerThrowsException_FollowsConfiguredExecutionMode`
  - `FailureInjection_AggregateException_CapturesAllFaultedHandlers`
  - `FailureInjection_OutOfMemorySimulation_PipelineCleanUp`
- [PublisherReentrancyTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Reliability/PublisherReentrancyTests.cs)
  - `PublisherReentrancy_ExceedingDepthLimit_ThrowsReentrancyLimitException`
  - `PublisherReentrancy_NestedPublish_DecrementsDepthOnCompletion`
- [ReentrancyLeakOnUnregisteredEventTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Reliability/ReentrancyLeakOnUnregisteredEventTests.cs)
  - `Reentrancy_WhenEventHasNoRegisteredHandlers_DecrementsDepthGracefully`
- [ExecutionStrategyContractDivergenceTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Reliability/ExecutionStrategyContractDivergenceTests.cs)
  - `ExecutionStrategy_CancellationDivergence_HonorsCancellationToken`
  - `ExecutionStrategy_ExceptionPropagation_PreservesStackTrace`
- [ForensicAdversarialEvidenceTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/ForensicAdversarialEvidenceTests.cs)
  - `EVT_ISO_001_InMemoryEventPublisher_ReentrancyDepth_IsIsolatedPerInstance`

## Execution
```powershell
dotnet test tests/EricksonLopez.Events.UnitTests --filter "FullyQualifiedName~Reliability"
```
