# Reliability Tests — Audit-Generated

## Required Tests

### TransactionalPublisher_CommitAndPublish_OnHandlerFailure_EventRemainsInBuffer
Reproduces HIGH-002 scenario 4: handler throws during CommitAndPublishAsync.
Assert: after fix with Queue<T>, event is NOT re-published on retry.

### EventBus_StoppingToken_PropagatedToHandlers
Shutdown scenario: pass stoppingToken to PublishAsync.
Assert: handler receives canceled token and terminates cleanly.
