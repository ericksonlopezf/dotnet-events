# 16. CANCELLATION & CANCELLATIONTOKEN PROPAGATION AUDIT

## 1. EARLY PRE-CANCELLATION CHECK
In `InMemoryEventPublisher.cs` and `EventPublisher.cs`:
```csharp
if (cancellationToken.IsCancellationRequested)
{
    return ValueTask.FromCanceled(cancellationToken);
}
```
If the token is canceled prior to starting publication, the method immediately returns a canceled `ValueTask` without allocating scopes, resolving services, or executing middlewares and handlers.

---

## 2. INTER-HANDLER CANCELLATION
Within `SequentialExecutionStrategy`, the token is inspected before dispatching each successive handler:
- If canceled during Handler 1, Handler 2 is **never invoked**.
- `OperationCanceledException` is thrown cleanly, ensuring no orphaned tasks persist in the pipeline.