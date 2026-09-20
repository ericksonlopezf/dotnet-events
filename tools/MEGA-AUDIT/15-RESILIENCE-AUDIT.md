# 15. RESILIENCE & RETRY POLICY AUDIT

## 1. IN-MEMORY VS DISTRIBUTED RETRY POLICIES
- **In-Memory Bus**: Retrying in-memory on transient exceptions (e.g., database lock contention) must be managed carefully to avoid ThreadPool thread starvation.
- **Exception Isolation**: In the sequential execution strategy, exceptions bubble up immediately to avoid misleading publishers into assuming all downstream subscribers completed successfully.

---

## 2. INTEGRATION WITH `EricksonLopez.Resilience.Polly`
Rather than introducing ad-hoc retry mechanisms (with potential bugs in jitter, exponential backoff, or circuit breaking), `EricksonLopez.Events` provides middleware hooks (`IEventMiddleware`) where battle-tested resilience policies from **`EricksonLopez.Resilience.Polly`** can be injected.