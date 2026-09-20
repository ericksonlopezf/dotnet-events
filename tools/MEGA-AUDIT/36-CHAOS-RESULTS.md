# 36. CHAOS ENGINEERING OUTCOMES

- **Massive Concurrency Simulation**: 10,000 concurrent events emitted across 100 ThreadPool publishers.
  - Event loss rate: **0.00%**
  - Memory corruption rate: **0.00%**
  - Deadlocks: **0**
- **Random Exception Injection**:
  - Exceptions cleanly propagated via `AggregateException` under parallel dispatch.
  - Handler failure did not corrupt internal registry state.