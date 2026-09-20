# Chaos Tests — Audit-Generated

## Status
Directory exists. Tests not yet included in active solution.

## Required Chaos Scenarios

### ChaoticHandlerFailure_30PercentRate_FailFast
Random 30% handler failure under FailFast policy.
Assert: exception propagates immediately, other handlers not called.

### ChaoticHandlerFailure_30PercentRate_ContinueOnError
Random 30% handler failure under ContinueOnError policy.
Assert: EventDispatchException contains all failures.

### BurstTraffic_10000Events_NoDeadlock
10,000 concurrent events published in burst.
Assert: completes within 30s, no deadlock.

### RandomCancellations_NoOrphanedTasks
Random CancellationToken cancellations mid-pipeline.
Assert: no leaked tasks, proper cleanup.
