# 08. TRANSACTIONAL CONSISTENCY AUDIT

## 1. ANALYSIS OF `TransactionalEventPublisher`

The `TransactionalEventPublisher` component was originally designed as an in-memory buffering publisher to retain events until database commit:

```csharp
public async ValueTask CommitAndPublishAsync(CancellationToken cancellationToken = default)
{
    var eventsToPublish = _pendingEvents.ToArray();
    _pendingEvents.Clear();
    foreach (var @event in eventsToPublish)
    {
        await _innerPublisher.PublishAsync(@event, cancellationToken).ConfigureAwait(false);
    }
}
```

---

## 2. FORENSIC DEFECT: THE CRASH-BETWEEN-COMMIT-AND-PUBLISH PROBLEM

An unavoidable vulnerability exists in any purely in-memory buffer:
1. Application begins a database transaction.
2. Application calls `publisher.PublishAsync(@event)`, appending the event to `_pendingEvents`.
3. Database executes `dbTransaction.Commit()` successfully.
4. **CATASTROPHIC FAILURE**: At this precise millisecond, the container crashes (`OOMKilled`), VM restarts, or power fails.
5. **OUTCOME**: Business state changes are committed to the database, but in-memory events are lost forever. Downstream subscribers will never receive notification.

---

## 3. MANDATORY INTEGRATION WITH `EricksonLopez.Transaction` AND `Outbox`
To achieve true atomic consistency between state changes and event publication, the architecture must mandate the use of **`EricksonLopez.Outbox`**, where events are inserted into the same database transaction managed by **`EricksonLopez.Transaction`**.