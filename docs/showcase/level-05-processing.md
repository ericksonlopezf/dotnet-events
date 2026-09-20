# Level 05 — Processing: Event Registries & Execution Strategies

> **Showcase Level 5** | Reference: `ECommerce.App/Program.cs` (`RunLevel5RegistryAndBehaviorAsync`)

---

## 1. Overview

In Level 05, we explore the event catalog infrastructure and dispatch processing controls. We demonstrate type descriptor resolution using `IEventTypeRegistry`, high-performance static lookups with `StaticEventTypeRegistry`, reentrancy safeguards, and concurrency execution modes (`Sequential` vs. `Parallel`).

---

## 2. Event Type Registry & Descriptors

`EventTypeRegistryBuilder` dynamically or statically indexes event types for polymorphic routing:

```csharp
using System;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Registry;
using ECommerce.Application;

var builder = new EventTypeRegistryBuilder();

// 1. Fluent registration
builder.Register<OrderPlacedIntegrationEvent>();
builder.Register<OrderShippedIntegrationEvent>();
builder.Register<CustomerRegisteredIntegrationEvent>();

// 2. Build immutable registry
IEventTypeRegistry registry = builder.Build();

// 3. Lookup descriptor by CLR Type (AOT-safe)
if (registry.TryGetDescriptor<OrderPlacedIntegrationEvent>(out var desc1))
{
    Console.WriteLine($"Descriptor resolved: CLR={desc1.ClrType.Name}, EventType={desc1.EventType}, Version=v{desc1.Version}");
}

// 4. Lookup descriptor by string / EventType
if (registry.TryGetDescriptor(EventType.From("ecommerce.orders.order-shipped"), out var desc2))
{
    Console.WriteLine($"Descriptor resolved by string: CLR={desc2.ClrType.Name}");
}
```

---

## 3. Zero-Allocation Static Lookups: StaticEventTypeRegistry

`StaticEventTypeRegistry` provides compile-time cached descriptors without hash table lookup overhead:

```csharp
using EricksonLopez.Events.Registry;
using ECommerce.Application;

// Assign active registry
StaticEventTypeRegistry.SetCurrent(registry, allowOverride: true);

// Fast generic lookups
var descriptor = StaticEventTypeRegistry.GetDescriptor<OrderPlacedIntegrationEvent>();
var eventType  = StaticEventTypeRegistry.GetEventType<OrderPlacedIntegrationEvent>();
var version    = StaticEventTypeRegistry.GetVersion<OrderPlacedIntegrationEvent>();

// Freeze registry against unauthorized modifications in production
StaticEventTypeRegistry.Freeze();
bool isFrozen = StaticEventTypeRegistry.IsFrozen; // True
```

---

## 4. Concurrency Execution Modes & Reentrancy Guards

In `EventBusOptions`, handlers can be executed sequentially or concurrently:

```csharp
using EricksonLopez.Events.Bus.Configuration;

var options = new EventBusOptions
{
    // Sequential: Handlers run in declaration order, sharing the ambient scope
    ExecutionMode = EventExecutionMode.Sequential,

    // Parallel: Handlers run concurrently via Task.WhenAll with isolated scopes
    // ExecutionMode = EventExecutionMode.Parallel,
    // MaxDegreeOfParallelism = 4,

    // Guard against infinite recursive event loops (A -> B -> A)
    MaxReentrancyDepth = 5,

    // Scoping policy
    ScopePolicy = HandlerScopePolicy.Auto
};

options.Validate();
```
