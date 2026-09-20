# Level 03 — Real Use Cases: In-Memory Event Publisher & Subscribers

> **Showcase Level 3** | Reference: `ECommerce.App/Program.cs` (`RunLevel3InMemoryPublisherAsync`)

---

## 1. Overview

In Level 03, we demonstrate lightweight, in-process event publishing using `InMemoryEventPublisher`. This implementation satisfies both `IEventBus` (publishing) and `IEventSubscriber` (runtime subscriptions), making it ideal for unit tests, console tools, and modular systems that operate without a dependency injection container.

---

## 2. Implementing Handlers

Handlers implement `IEventHandler<TEvent>`, returning `ValueTask` for zero-allocation asynchronous execution:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;
using ECommerce.Domain;

public sealed class InventoryStockAllocationHandler : IEventHandler<OrderPlacedDomainEvent>
{
    public ValueTask HandleAsync(OrderPlacedDomainEvent eventInstance, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Inventory] Allocating stock for Order {eventInstance.OrderId.Value} ({eventInstance.TotalAmount.Amount} {eventInstance.TotalAmount.Currency})");
        return ValueTask.CompletedTask;
    }
}

public sealed class CustomerNotificationHandler : IEventHandler<OrderPlacedDomainEvent>
{
    public ValueTask HandleAsync(OrderPlacedDomainEvent eventInstance, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Notification] Sending confirmation to Customer {eventInstance.CustomerId.Value}");
        return ValueTask.CompletedTask;
    }
}
```

---

## 3. Subscribing, Publishing, and Unsubscribing

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Dispatch;
using ECommerce.Domain;

var publisher = new InMemoryEventPublisher();

var inventoryHandler = new InventoryStockAllocationHandler();
var notificationHandler = new CustomerNotificationHandler();

// 1. Subscribe handlers dynamically
publisher.Subscribe(inventoryHandler);
publisher.Subscribe(notificationHandler);

// 2. Publish event to all subscribers
var orderPlaced = new OrderPlacedDomainEvent(
    EventId.New(),
    OrderId.New(),
    CustomerId.New(),
    Money.USD(199.95m),
    DateTimeOffset.UtcNow);

await publisher.PublishAsync(orderPlaced, CancellationToken.None);

// 3. Unsubscribe a handler dynamically
publisher.Unsubscribe(inventoryHandler);

// 4. Publish again — only notificationHandler receives this event
await publisher.PublishAsync(orderPlaced, CancellationToken.None);
```
