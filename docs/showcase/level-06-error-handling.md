# Level 06 — Error Handling & Resilience

> **Showcase Level 6** | Reference: `ECommerce.App/Program.cs` (`RunLevel6ErrorHandlingAsync`)

---

## 1. Overview

In Level 06, we demonstrate resilience strategies and error isolation policies during event dispatching. We explore the trade-offs between `FailFast` and `AggregateAndContinue`, and examine the core exception hierarchy (`EventDispatchException`, `EventTypeNotFoundException`, `EventValidationException`).

---

## 2. Error Handling Policies

| Policy | Behavior | When to Use |
|---|---|---|
| **`FailFast`** | First handler exception aborts dispatch immediately. Subsequent handlers are not invoked. | Critical transactional invariants where partial processing is unacceptable. |
| **`AggregateAndContinue`** | Faulting handlers do not interrupt sibling handlers. All subscribers are executed, and all exceptions are gathered. | Decoupled notifications, auditing, and multi-subscriber event meshes. |

---

## 3. Gathering Failures with EventDispatchException

When configured with `ErrorHandlingPolicy.AggregateAndContinue`, the bus captures all handler faults and throws a unified `EventDispatchException`:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using ECommerce.Application;

var services = new ServiceCollection();
services.AddEventBus(options =>
{
    options.ExecutionMode = EventExecutionMode.Sequential;
    options.ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue;
});

// Handlers: one will fail, one will succeed
services.AddEventHandler<OrderPlacedIntegrationEvent, SendOrderConfirmationEmailHandler>();
services.AddEventHandler<OrderPlacedIntegrationEvent, FailingPaymentProcessingHandler>();

using var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

var evt = new OrderPlacedIntegrationEvent(
    EventId.New(), Guid.NewGuid(), Guid.NewGuid(), 100m, "USD", DateTimeOffset.UtcNow);

try
{
    await bus.PublishAsync(evt, CancellationToken.None);
}
catch (EventDispatchException dispatchEx)
{
    Console.WriteLine($"Dispatch failed for event type: {dispatchEx.EventType}");
    Console.WriteLine($"Total exceptions caught: {dispatchEx.InnerExceptions.Count}");

    foreach (var inner in dispatchEx.InnerExceptions)
    {
        Console.WriteLine($" - [{inner.GetType().Name}]: {inner.Message}");
    }
}
```

---

## 4. Unregistered Event Protection: EventTypeNotFoundException

When `ThrowOnUnregisteredEvent` is enabled, dispatching an event type absent from the registry throws `EventTypeNotFoundException`:

```csharp
using EricksonLopez.Events.Exceptions;
using EricksonLopez.Events.Identifiers;

try
{
    // Thrown when an unknown event type cannot be resolved
    throw new EventTypeNotFoundException(EventType.From("unregistered.billing.event"));
}
catch (EventTypeNotFoundException ex)
{
    Console.WriteLine($"Unregistered event detected: {ex.EventType}");
}
```
