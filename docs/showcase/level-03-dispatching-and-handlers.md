# Level 03 — In-Process Dispatching & Handlers

In Level 03, we explore high-performance in-process event publishing and subscription using `IEventPublisher` and `IEventHandler<T>`.

---

## 1. Implementing an Event Handler

```csharp
using EricksonLopez.Events.Contracts;

public sealed class SendOrderConfirmationEmailHandler : IEventHandler<OrderPlacedDomainEvent>
{
    private readonly IEmailService _emailService;

    public SendOrderConfirmationEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(
        EventEnvelope<OrderPlacedDomainEvent> envelope, 
        CancellationToken cancellationToken = default)
    {
        var order = envelope.Payload;
        await _emailService.SendAsync(
            recipientId: order.CustomerId,
            subject: "Order Confirmation",
            body: $"Order {order.OrderId} placed for ${order.TotalAmount}",
            cancellationToken: cancellationToken);
    }
}
```

---

## 2. Registering and Publishing Events

```csharp
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Events;

var services = new ServiceCollection();
services.AddEvents();
services.AddEventHandler<SendOrderConfirmationEmailHandler, OrderPlacedDomainEvent>();

var provider = services.BuildServiceProvider();
var publisher = provider.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(envelope, CancellationToken.None);
```
