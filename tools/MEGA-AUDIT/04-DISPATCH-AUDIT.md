# 04. EVENT DISPATCH & ROUTING AUDIT

## 1. EXECUTION STRATEGIES

The `EricksonLopez.Events` engine implements two dispatch strategies via the `IExecutionStrategy` abstraction:

### A. `SequentialExecutionStrategy`
- **Behavior**: Iterates sequentially through registered handlers for the event type in deterministic registration order.
- **Guarantees**:
  - Strict ordering determinism.
  - If a handler throws an unhandled exception, execution halts immediately, preserving pipeline consistency.
  - Clean `CancellationToken` propagation at every step.
- **Performance**: In-memory dispatch of 1 handler in **87.93 ns** (312 bytes allocated on the Managed Heap).

### B. `ParallelExecutionStrategy`
- **Behavior**: Executes registered handlers concurrently using `Task.WhenAll`.
- **Guarantees**:
  - Minimizes aggregate latency when multiple handlers perform I/O-bound work.
  - Exception isolation: If multiple handlers fail, exceptions are aggregated into an `AggregateException`.
- **Identified Risk**: When using `HandlerScopePolicy.ReuseAmbientScope`, concurrent parallel tasks share the same `IServiceProvider` and may trigger concurrency `InvalidOperationException` on non-thread-safe dependencies like EF Core `DbContext`.

---

## 2. HANDLER RESOLUTION (`HandlerResolutionHelper`)

The `ResolveHandlersAsync` method extracts descriptors from the registry and manages dependency injection scopes:
- If the descriptor contains a registered concrete `HandlerType`, it resolves the concrete instance from DI.
- If registered without a specific descriptor, it resolves `IEnumerable<IEventHandler<T>>`.
- **Performance Defect Addressed**: Documents the $O(N^2)$ instantiation storm when `AddHandler<T>()` is omitted in DI (see Finding `EVT-LFC-001`).