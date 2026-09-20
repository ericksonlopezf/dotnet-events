# Architecture Diagrams — EricksonLopez.Events

All diagrams are auto-generated from the public API surface as of the last Showcase synchronization.
The Showcase at `samples/ECommerce.Sample/ECommerce.App` is the **executable source of truth**.

---

## 1. Package Dependency Graph

```mermaid
graph TD
    Contracts["EricksonLopez.Events.Contracts\nIEvent · IDomainEvent · IIntegrationEvent\nIEventHandler · IEventPublisher · IEventBus\nIdentifiers · Attributes · Envelopes · Metadata"]

    Core["EricksonLopez.Events\nEventBus · Middleware · Registry\nDI Extensions · Diagnostics · Dispatch"]

    CloudEvents["EricksonLopez.Events.CloudEvents\nCloudEvent&lt;TData&gt;\nToCloudEvent() · ToEventEnvelope()"]

    OTel["EricksonLopez.Events.OpenTelemetry\nAddEventsInstrumentation()\nTracerProvider · MeterProvider"]

    STJ["EricksonLopez.Events.Serialization.SystemTextJson\nAddEventsConverters()\nCreateDefaultOptions()\n8 typed JSON converters"]

    Testing["EricksonLopez.Events.Testing\nFakeEventPublisher\nTestEventHandler&lt;T&gt;\nEventTestBuilder"]

    Generators["EricksonLopez.Events.Generators\nRoslyn Source Generator\nGeneratedEventRegistry\nAOT handler registration"]

    Contracts --> Core
    Contracts --> CloudEvents
    Contracts --> STJ
    Contracts --> Testing
    Core --> OTel
    Core --> STJ
```

---

## 2. End-to-End Event Dispatch Lifecycle

```mermaid
sequenceDiagram
    autonumber
    actor Caller as Application Service / Aggregate
    participant Bus as IEventBus (EventBus)
    participant Diag as EventsDiagnostics
    participant Ctx as EventContext (AsyncLocal)
    participant Pipe as MiddlewarePipeline
    participant Strat as IExecutionStrategy
    participant Scope as IServiceScope
    participant Handler as IEventHandler&lt;T&gt;

    Caller->>Bus: PublishAsync&lt;TEvent&gt;(event, ct)
    Bus->>Bus: Check MaxReentrancyDepth (AsyncLocal)
    Bus->>Diag: StartPublishActivity(event)
    activate Diag
    Bus->>Ctx: SetCurrent(envelope)
    activate Ctx
    Bus->>Diag: RecordEventPublished(eventTypeName)

    Bus->>Pipe: Build pipeline from IEventMiddleware[]
    activate Pipe
    Note over Pipe: Middleware 1 to N to Terminal

    Pipe->>Strat: ExecuteAsync(handlers, event, sp, options, ct)
    activate Strat

    alt ExecutionMode == Sequential
        loop Each Handler
            Strat->>Scope: CreateScope() if ScopePolicy != ReuseAmbientScope
            Scope->>Handler: HandleAsync(event, ct)
            Handler-->>Strat: ValueTask done
            Strat->>Diag: RecordEventHandled(name, ms, success)
        end
    else ExecutionMode == Parallel
        Note over Strat: Parallel.ForEachAsync (MaxDegreeOfParallelism)
        par Concurrent
            Strat->>Scope: CreateScope()
            Scope->>Handler: HandleAsync(event, ct)
        and
            Strat->>Scope: CreateScope()
            Scope->>Handler: HandleAsync(event, ct)
        end
        Strat->>Diag: RecordEventHandled per handler
    end

    Strat-->>Pipe: done
    deactivate Strat
    Pipe-->>Bus: done
    deactivate Pipe
    Ctx-->>Bus: restore
    deactivate Ctx
    Diag-->>Bus: stop activity
    deactivate Diag
    Bus-->>Caller: ValueTask completed
```

---

## 3. Middleware Pipeline Construction

```mermaid
flowchart LR
    subgraph Build["MiddlewarePipeline.Build&lt;TEvent&gt;(middlewares, terminal)"]
        direction LR
        T["Terminal (EventMiddlewareDelegate&lt;T&gt;)"]
        M2["Middleware N (IEventMiddleware)"]
        M1["Middleware 2 (IEventMiddleware)"]
        M0["Middleware 1 (IEventMiddleware)"]
        M0 -->|"InvokeAsync(evt, next, ct)"| M1
        M1 -->|"InvokeAsync(evt, next, ct)"| M2
        M2 -->|"InvokeAsync(evt, next, ct)"| T
    end

    Caller["EventBus.PublishAsync()"] --> M0
    T --> Strat["IExecutionStrategy.ExecuteAsync()"]
```

---

## 4. StaticEventTypeRegistry State Machine

```mermaid
stateDiagram-v2
    [*] --> Empty: initial state (EventTypeRegistry.Empty)
    Empty --> Initialized: SetCurrent(registry, allowOverride=false)
    Empty --> Initialized: SetCurrent(registry, allowOverride=true)
    Initialized --> Initialized: SetCurrent(registry, allowOverride=true)
    Initialized --> Empty: Reset() — not frozen
    Initialized --> Frozen: Freeze()
    Frozen --> Frozen: IsFrozen = true
    Initialized --> Error_AlreadyInit: SetCurrent() allowOverride=false already initialized
    Frozen --> Error_Frozen: SetCurrent() or Reset() when frozen
```

---

## 5. Error Handling Policy Decision Tree

```mermaid
flowchart TD
    Dispatch["EventBus dispatches event to N handlers"]
    Dispatch --> H1["Handler 1 — OK"]
    Dispatch --> H2["Handler 2 — FAIL"]
    Dispatch --> H3["Handler 3 — OK"]
    H2 --> Policy{ErrorHandlingPolicy}
    Policy -->|FailFast| FF["Stop immediately. Rethrow first exception. Remaining handlers DO NOT run"]
    Policy -->|AggregateAndContinue| Agg["Continue remaining handlers. Collect all exceptions. Throw EventDispatchException"]
    FF --> FFResult["Caller catches original exception type"]
    Agg --> AggResult["Caller catches EventDispatchException with InnerExceptions list"]
```

---

## 6. EventEnvelope to CloudEvent Roundtrip

```mermaid
flowchart LR
    subgraph Domain["In-Process Domain"]
        Event["IIntegrationEvent (TEvent)"]
        Meta["EventMetadata"]
        Envelope["EventEnvelope&lt;TEvent&gt;"]
    end
    subgraph CloudEventsSpec["CloudEvents v1.0 CNCF"]
        CE["CloudEvent&lt;TData&gt; specversion:1.0"]
    end
    subgraph Transport["Broker / HTTP"]
        JSON["JSON payload"]
    end
    Event --> Envelope
    Meta --> Envelope
    Envelope -->|"ToCloudEvent(defaultSource)"| CE
    CE -->|"ToEventEnvelope()"| Envelope
    CE -->|"ConfigureForCloudEvents()"| JSON
    JSON --> CE
```
