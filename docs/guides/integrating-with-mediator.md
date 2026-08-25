# Guide: Integrating EricksonLopez.Events with EricksonLopez.Mediator

`EricksonLopez.Events` defines pure event contracts, identities, and envelopes. It intentionally excludes in-process pipeline behaviors, DI handler resolution, and request/response dispatch, which are the exclusive responsibility of `EricksonLopez.Mediator`.

This guide outlines how to bridge `EricksonLopez.Events` with `EricksonLopez.Mediator` in production applications.

---

## 1. Architectural Boundary & Separation of Concerns

| Concern | EricksonLopez.Events | EricksonLopez.Mediator |
|---|:---:|:---:|
| Event Identity (`EventId`, `EventType`, `EventVersion`) | Yes | No |
| Ambient Metadata (`CorrelationId`, `TenantId`) | Yes | Consumes |
| Event Envelope (`EventEnvelope<TEvent>`) | Yes | Consumes |
| `IEventHandler<TEvent>` Interface | Defined | Implemented / Routed |
| `IEventPublisher` Interface | Defined | Implemented / Dispatched |
| DI Lifetime Management & Scoping | No | Yes |
| Middleware Pipeline (Validation, Logging, Retry) | No | Yes |
| Request / Response (Commands & Queries) | No | Yes |

---

## 2. Implementing `IEventPublisher` via Mediator

In production systems, `IEventPublisher` should be implemented by your mediator dispatcher:

```csharp
namespace MyApp.Infrastructure.Dispatch;

using EricksonLopez.Events.Contracts;
using EricksonLopez.Mediator;

public sealed class MediatorEventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;

    public MediatorEventPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);

        // Dispatches through the mediator pipeline with validation, logging, and metrics
        await _mediator.PublishAsync(eventInstance, cancellationToken).ConfigureAwait(false);
    }
}
```

---

## 3. Registering Event Handlers in Dependency Injection

Event handlers implement `IEventHandler<TEvent>`:

```csharp
namespace MyApp.Application.Handlers;

using EricksonLopez.Events.Contracts;
using MyApp.Domain.Events;

public sealed class SendOrderConfirmationEmailHandler : IEventHandler<OrderPlacedEvent>
{
    private readonly IEmailService _emailService;

    public SendOrderConfirmationEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async ValueTask HandleAsync(OrderPlacedEvent eventInstance, CancellationToken cancellationToken = default)
    {
        await _emailService.SendReceiptAsync(eventInstance.OrderId, eventInstance.Amount, cancellationToken);
    }
}
```

In your DI container registration (e.g. Microsoft.Extensions.DependencyInjection):

```csharp
public static IServiceCollection AddApplicationEvents(this IServiceCollection services)
{
    services.AddScoped<IEventHandler<OrderPlacedEvent>, SendOrderConfirmationEmailHandler>();
    services.AddScoped<IEventPublisher, MediatorEventPublisher>();
    return services;
}
```

---

## 4. Pipeline Behaviors & Tracing

When an event is published via `MediatorEventPublisher`:
1. OpenTelemetry traces automatically propagate the `CorrelationId` and `CausationId` from `EventMetadata`.
2. Pipeline behaviors intercept the event for authorization, transaction commits, or telemetry logging.
3. Multiple `IEventHandler<TEvent>` instances are resolved from the scoped container and invoked concurrently or sequentially depending on mediator configuration.
