# Level 04 — Advanced Integration: Microsoft DI & EventBus

> **Showcase Level 4** | Reference: `ECommerce.App/Program.cs` (`RunLevel4AdvancedIntegrationAsync`)

---

## 1. Overview

In Level 04, we integrate `EricksonLopez.Events` with `Microsoft.Extensions.DependencyInjection`. We demonstrate how `EventBus` orchestrates decoupled handler execution with automatic dependency resolution, lifecycle scoping, and typed envelope publishing.

---

## 2. Configuring Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using ECommerce.Application;
using ECommerce.Domain;

var services = new ServiceCollection();

// 1. Register the EventBus core engine
services.AddEventBus(options =>
{
    options.ExecutionMode = EventExecutionMode.Sequential;
    options.ErrorPolicy = ErrorHandlingPolicy.FailFast;
    options.ThrowOnUnregisteredEvent = false;
    options.MaxReentrancyDepth = 10;
    options.ScopePolicy = HandlerScopePolicy.Auto;
});

// 2. Register typed event handlers
services.AddEventHandler<OrderPlacedIntegrationEvent, SendOrderConfirmationEmailHandler>(ServiceLifetime.Transient);
services.AddEventHandler<OrderPlacedIntegrationEvent, UpdateInventoryOnOrderPlacedHandler>(ServiceLifetime.Transient);

// 3. Register envelope handlers for metadata-aware consumers
services.AddEnvelopeEventHandler<OrderPlacedDomainEvent, SampleEnvelopeEventConsumer>(ServiceLifetime.Transient);

// 4. Build service provider
using var serviceProvider = services.BuildServiceProvider();
```

---

## 3. In-Process Dispatching via IEventBus

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using ECommerce.Application;

using var scope = serviceProvider.CreateScope();
var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

var integrationEvent = new OrderPlacedIntegrationEvent(
    EventId.New(),
    Guid.NewGuid(),
    Guid.NewGuid(),
    249.99m,
    "USD",
    DateTimeOffset.UtcNow);

// Direct event publish
await bus.PublishAsync(integrationEvent, CancellationToken.None);
```

---

## 4. Publishing Event Envelopes with Context

Using `EventPublisherExtensions.PublishAsync`, handlers can receive enriched metadata via ambient `EventContext`:

```csharp
var metadata = new EventMetadataBuilder()
    .WithCorrelationId(CorrelationId.New())
    .WithTenantId(TenantId.From("tenant-enterprise"))
    .WithSource("order-service")
    .Build();

var envelope = EventEnvelope.Create(integrationEvent, metadata);

// Publish envelope directly
await bus.PublishAsync(envelope, CancellationToken.None);
```
