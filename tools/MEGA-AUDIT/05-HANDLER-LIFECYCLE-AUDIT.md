# 05. HANDLER LIFECYCLE AUDIT

## 1. SCOPE POLICIES (`HandlerScopePolicy`)

`EventBusOptions` provides three configurable policies for dependency lifetime scopes:

```csharp
public enum HandlerScopePolicy
{
    CreateScopePerHandler = 0,
    ReuseAmbientScope = 1,
    NoScope = 2
}
```

### Forensic Policy Evaluation:
1. **`CreateScopePerHandler` (Default)**:
   - Creates a dedicated `IServiceScope` for each handler executing the event.
   - **Advantages**: Total isolation. Each handler receives its own Scoped dependencies (repositories, DbContext, UnitOfWork). Zero risk of concurrency or state leakage between subscribers.
   - **Cost**: Allocation of one scope object per handler invocation.

2. **`ReuseAmbientScope`**:
   - Reuses the active ambient scope from the current HTTP request or command pipeline.
   - **Risk**: Hazardous when combined with `ParallelExecutionStrategy`, as multiple concurrent tasks share the same `DbContext` instance.

3. **`NoScope`**:
   - Resolves handlers directly from the root container.
   - **Risk**: Captive dependencies if Transient or Scoped handlers are resolved from the Singleton root container.

---

## 2. FORENSIC FINDING EVT-LFC-001: $O(N^2)$ INSTANTIATION STORM

### Defect Description:
In `HandlerResolutionHelper.cs`, when handlers are registered generically in DI as `services.AddTransient<IEventHandler<T>, Handler>()` without concrete type tracking in `HandlerDescriptor`, the resolver executes:
```csharp
var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();
```
For each descriptor in the list of $N$ descriptors, the DI container resolves the entire collection of $N$ items. Consequently, for $N$ transient handlers, $N \times N = N^2$ objects are allocated in memory.

### Mitigation and Recommendation:
Standardize registration through the fluent API `services.AddEventBus(b => b.AddHandler<THandler>())`, which registers the concrete implementation type in the descriptor, resolving exactly one instance per descriptor ($O(N)$).