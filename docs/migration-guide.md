# Migration Guide — EricksonLopez.Events

Instructions for migrating existing systems from traditional event libraries or Guid v4-based identities to **EricksonLopez.Events**.

---

## 1. Migrating from `Guid.NewGuid()` to `EventId.New()` (Guid v7)

### Before:
```csharp
public class OrderPlacedEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
```

### After:
```csharp
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

public sealed record OrderPlacedEvent(
    EventId Id,
    DateTimeOffset OccurredAt) : IDomainEvent;

// Creation:
var evt = new OrderPlacedEvent(EventId.New(), DateTimeOffset.UtcNow);
```

---

## 2. Migrating from `INotificationHandler` (MediatR) to `IEventHandler`

### Before:
```csharp
public class OrderHandler : INotificationHandler<OrderNotification>
{
    public async Task Handle(OrderNotification notification, CancellationToken cancellationToken)
    {
        // ...
    }
}
```

### After:
```csharp
using EricksonLopez.Events.Contracts;

public sealed class OrderHandler : IEventHandler<OrderPlacedEvent>
{
    public ValueTask HandleAsync(OrderPlacedEvent eventInstance, CancellationToken cancellationToken = default)
    {
        // High-performance processing returning ValueTask
        return ValueTask.CompletedTask;
    }
}
```

---

## 3. Registering in the Dependency Injection Container

### Before:
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

### After:
```csharp
services.AddEventBus(options =>
{
    options.ExecutionMode = EventExecutionMode.Sequential;
    options.ErrorPolicy = ErrorHandlingPolicy.FailFast;
});

// Explicit, AOT-compatible registration
services.AddEventHandler<OrderPlacedEvent, OrderHandler>();

// Or using the Source Generator automatically:
// services.AddGeneratedEventHandlers();
```
