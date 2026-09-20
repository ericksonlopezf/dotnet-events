# Level 08 — Customization: Middlewares & Execution Strategies

> **Showcase Level 8** | Reference: `ECommerce.Infrastructure/Infrastructure.cs` & `ECommerce.App/Program.cs` (`RunLevel8CustomizationAndMiddlewaresAsync`)

---

## 1. Overview

In Level 08, we explore extensibility mechanisms: custom pipeline middlewares (`IEventMiddleware`) and specialized handler execution strategies (`IExecutionStrategy`). This enables cross-cutting capabilities such as distributed correlation enforcement, auditing, profiling, and custom concurrency orchestration.

---

## 2. Implementing Custom Middlewares: IEventMiddleware

Middlewares intercept event dispatching before and after handlers execute:

```csharp
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Contracts;

public sealed class LoggingEventMiddleware : IEventMiddleware
{
    public async ValueTask InvokeAsync<TEvent>(
        TEvent eventInstance,
        EventMiddlewareDelegate<TEvent> next,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine($"[Middleware] [LOG-START] Dispatching '{typeof(TEvent).Name}' (ID: {eventInstance.Id})");

        try
        {
            await next(eventInstance, cancellationToken);
            sw.Stop();
            Console.WriteLine($"[Middleware] [LOG-END] Handled in {sw.Elapsed.TotalMilliseconds:F3} ms.");
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.WriteLine($"[Middleware] [LOG-ERROR] Failed after {sw.Elapsed.TotalMilliseconds:F3} ms: {ex.Message}");
            throw;
        }
    }
}
```

---

## 3. Registering Middlewares in Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Middleware;

var services = new ServiceCollection();
services.AddEventBus();

// Register cross-cutting middlewares in execution order
services.AddEventMiddleware<LoggingEventMiddleware>();
services.AddEventMiddleware<CausationDepthLimitMiddleware>(); // Built-in guard
```

---

## 4. Manual Middleware Pipeline Construction: MiddlewarePipeline.Build

For high-performance scenarios or non-DI hosts, `MiddlewarePipeline.Build` constructs a compiled delegate chain:

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus.Middleware;
using ECommerce.Domain;

IReadOnlyList<IEventMiddleware> middlewares = [
    new LoggingEventMiddleware(),
    new CausationDepthLimitMiddleware(maxDepth: 5)
];

// Terminal delegate representing final handler execution
EventMiddlewareDelegate<OrderPlacedDomainEvent> terminal = (evt, ct) =>
{
    // Final handler dispatch logic
    return ValueTask.CompletedTask;
};

// Compile the pipeline
var pipeline = MiddlewarePipeline.Build(middlewares, terminal);

// Execute pipeline
await pipeline(orderPlacedEvent, CancellationToken.None);
```

---

## 5. Custom Execution Strategies: IExecutionStrategy

`IExecutionStrategy` controls how resolved handlers are invoked (e.g., custom batching, rate-limiting):

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Execution;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;

public sealed class VerboseSequentialExecutionStrategy : IExecutionStrategy
{
    public async ValueTask ExecuteAsync<TEvent>(
        IReadOnlyList<HandlerDescriptor> handlers,
        TEvent eventInstance,
        IServiceProvider serviceProvider,
        EventBusOptions options,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        for (int i = 0; i < handlers.Count; i++)
        {
            var descriptor = handlers[i];
            var instance = serviceProvider.GetService(descriptor.HandlerType)
                ?? throw new InvalidOperationException($"Cannot resolve handler {descriptor.HandlerType.Name}");

            await descriptor.Invoker(instance, eventInstance, cancellationToken);
        }
    }
}
```
