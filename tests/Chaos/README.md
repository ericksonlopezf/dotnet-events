# Chaos Test Artifacts & Regression Suite

## Overview
This directory indexes chaos injection scenarios for `EricksonLopez.Events`, including randomized handler latencies, transient exceptions, abrupt cancellation token triggers, and burst publishing traffic.

## Test Suites & Locations
- [AdversarialChaosAndFailureInjectionTests.cs](file:///d:/DevData/ericksonlopez.dev/dotnet-events/tests/EricksonLopez.Events.UnitTests/Adversarial/Reliability/AdversarialChaosAndFailureInjectionTests.cs)
  - `Chaos_RandomHandlerFailures_EnsuresDeterministicErrorHandling`: Randomly injects failures across 50 simulated handlers, verifying pipeline consistency.
  - `Chaos_RandomCancellation_EnsuresCleanCancellation`: Injects cancellation at non-deterministic checkpoints across the dispatch pipeline.
  - `Chaos_BurstTraffic_ZeroMemoryLeak`: Injects 1,000 bursts of asynchronous events with concurrent registration checks.

## Invariants Verified Under Chaos
1. **Zero State Corruption**: No internal state or dictionary corruption occurs when a handler faults abruptly.
2. **Context Scope Cleanup**: Ambient `EventContext` scopes are guaranteed to dispose in `finally` blocks, preventing thread pollution.
3. **Cancellation Completeness**: If cancellation triggers, pending background tasks and semaphores are cleanly released without orphan leaks.

## Execution
```powershell
dotnet test tests/EricksonLopez.Events.UnitTests --filter "FullyQualifiedName~Chaos"
```
