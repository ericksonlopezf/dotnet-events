# 07. RESILIENCE & DELIVERY SEMANTICS AUDIT

## 1. REAL DELIVERY SEMANTICS

Classifying an in-memory bus as "Exactly-Once" is an architectural fallacy.
This forensic audit formally establishes the actual guarantees of the system:

| Dispatch Scenario | Actual Semantics | Process Crash Behavior | Exception Behavior |
| :--- | :--- | :--- | :--- |
| **In-Memory Sequential** | **At-Least-Once (In-Process)** | Total loss of unprocessed events in memory. | Stops on first failure; previous handlers already executed. |
| **In-Memory Parallel** | **Best-Effort (In-Process)** | Total loss of in-flight events. | All handlers triggered; failures aggregated in `AggregateException`. |
| **Outbox Integration** | **Durable At-Least-Once** | Zero loss. Recovery after restart from database tables. | Configurable retries with exponential backoff and DLQ. |

---

## 2. FAULT INJECTION AND ADVERSARIAL TESTING
During the audit, faults were deliberately injected:
1. `OperationCanceledException`: Halts sequential dispatch cleanly without invoking subsequent handlers; propagates to the caller.
2. `OutOfMemoryException`: Does not corrupt the static type registry.
3. `AggregateException`: Under parallel dispatch, individual handler failures do not abort sibling handlers already in flight.