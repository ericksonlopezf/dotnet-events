# Getting Started — EricksonLopez.Events

A practical 5-minute guide to start using **EricksonLopez.Events** in .NET applications.

---

## 1. Package Installation

Install the main NuGet packages in your projects based on your architectural layer:

```bash
# In your Domain project
dotnet add package EricksonLopez.Events.Contracts

# In your Application / Host project (API or Console)
dotnet add package EricksonLopez.Events
dotnet add package EricksonLopez.Events.Serialization.SystemTextJson
```

---

## 2. Define Your First Event

Create an immutable domain or integration event:

```csharp
using System;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

namespace MyApp.Events;

[EventName("users.user-registered")]
[EventVersion(1)]
[EventSource("identity-service")]
public sealed record UserRegisteredEvent(
    EventId Id,
    string UserId,
    string Email,
    DateTimeOffset OccurredAt) : IEvent;
```

---

## 3. Create a Handler

Implement the `IEventHandler<TEvent>` interface:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;

namespace MyApp.Handlers;

public sealed class SendWelcomeEmailHandler : IEventHandler<UserRegisteredEvent>
{
    public ValueTask HandleAsync(UserRegisteredEvent eventInstance, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Email] Sending welcome to: {eventInstance.Email} (ID: {eventInstance.UserId})");
        return ValueTask.CompletedTask;
    }
}
```

---

## 4. Configure Dependency Injection

Register the bus and handlers in `Program.cs`:

```csharp
using EricksonLopez.Events.Bus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Events;
using MyApp.Handlers;

var services = new ServiceCollection();

// 1. Register the EventBus
services.AddEventBus();

// 2. Register the handler
services.AddEventHandler<UserRegisteredEvent, SendWelcomeEmailHandler>();

using var serviceProvider = services.BuildServiceProvider();
```

---

## 5. Publish an Event

Resolve `IEventBus` and dispatch the event:

```csharp
using System;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;

using var scope = serviceProvider.CreateScope();
var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

var evt = new UserRegisteredEvent(
    EventId.New(), // Guid v7
    "usr_42",
    "developer@example.com",
    DateTimeOffset.UtcNow);

await bus.PublishAsync(evt);
```

---

## Next Steps
- Explore the [Official Cookbook](cookbook.md) for recipes and advanced patterns.
- Browse the [API Reference](api-reference.md) for all methods and data structures.
- Run the [Showcase](showcase-guide.md) (`samples/ECommerce.Sample`) to see all 10 levels in action.
