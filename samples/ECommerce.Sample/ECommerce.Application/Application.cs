// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Application;

using ECommerce.Domain;
using EricksonLopez.Events.Attributes;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

// ─────────────────────────────────────────────────────────────────────────────
// Integration Events (IIntegrationEvent)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Integration event raised when a new order is placed.
/// Decorated with [EventName], [EventVersion], [EventSource] attributes
/// so the Source Generator and StaticEventTypeRegistry resolve it automatically.
/// </summary>
[EventName("ecommerce.orders.order-placed")]
[EventVersion(1)]
[EventSource("ordering-service")]
public sealed record OrderPlacedIntegrationEvent(
    EventId Id,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

/// <summary>
/// Integration event raised when an order is shipped.
/// Demonstrates a second integration event type with a different name and version.
/// </summary>
[EventName("ecommerce.orders.order-shipped")]
[EventVersion(1)]
[EventSource("fulfillment-service")]
public sealed record OrderShippedIntegrationEvent(
    EventId Id,
    Guid OrderId,
    string TrackingNumber,
    string CarrierCode,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

/// <summary>
/// Integration event raised when a customer is registered.
/// Used to demonstrate multiple event types in the registry.
/// </summary>
[EventName("ecommerce.customers.customer-registered")]
[EventVersion(1)]
[EventSource("customer-service")]
public sealed record CustomerRegisteredIntegrationEvent(
    EventId Id,
    Guid CustomerId,
    string Email,
    DateTimeOffset OccurredAt) : IIntegrationEvent;

// ─────────────────────────────────────────────────────────────────────────────
// Outbox Service Abstraction (Showcase-local, not EricksonLopez.Outbox)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Abstraction for storing events in a transactional outbox within the showcase.
/// This is a showcase-local abstraction — it is NOT EricksonLopez.Outbox.
/// </summary>
public interface IOutboxService
{
    ValueTask EnqueueAsync<TEvent>(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}

// ─────────────────────────────────────────────────────────────────────────────
// Application Services
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Application service for order placement.
/// Demonstrates the full DDD + Outbox pattern using the EricksonLopez.Events API.
/// </summary>
public sealed class OrderApplicationService
{
    private readonly IOutboxService _outboxService;

    public OrderApplicationService(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    public async Task<OrderId> PlaceOrderAsync(
        CustomerId customerId,
        decimal amount,
        CorrelationId correlationId,
        CancellationToken cancellationToken = default)
    {
        var orderId = OrderId.New();
        var order = new Order(orderId, customerId, Money.USD(amount));

        foreach (var domainEvent in order.DomainEvents)
        {
            if (domainEvent is OrderPlacedDomainEvent placed)
            {
                var integrationEvent = new OrderPlacedIntegrationEvent(
                    EventId.New(),
                    placed.OrderId.Value,
                    placed.CustomerId.Value,
                    placed.TotalAmount.Amount,
                    placed.TotalAmount.Currency,
                    placed.OccurredAt);

                // EventMetadataBuilder — full fluent API demo
                var metadata = new EventMetadataBuilder()
                    .WithCorrelationId(correlationId)
                    .WithCausationId(CausationId.From(placed.Id))
                    .WithSource("ordering-service")
                    .Build();

                var envelope = EventEnvelope.Create(integrationEvent, metadata);

                await _outboxService.EnqueueAsync(envelope, cancellationToken);
            }
        }

        order.ClearDomainEvents();
        return orderId;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Event Handlers — OrderPlacedIntegrationEvent
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Sends an order confirmation email when an order is placed.
/// Implements IEventHandler&lt;TEvent&gt; with the standard ValueTask pattern.
/// </summary>
public sealed class SendOrderConfirmationEmailHandler : IEventHandler<OrderPlacedIntegrationEvent>
{
    public bool Handled { get; private set; }
    public string? LastRecipientId { get; private set; }

    public ValueTask HandleAsync(OrderPlacedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        Handled = true;
        LastRecipientId = eventInstance.CustomerId.ToString();
        Console.WriteLine($"      [SendOrderConfirmationEmailHandler] Sending confirmation email for Order '{eventInstance.OrderId}' (Total: {eventInstance.TotalAmount} {eventInstance.Currency}).");
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Updates inventory when an order is placed.
/// Demonstrates multiple handlers registered for the same event type.
/// </summary>
public sealed class UpdateInventoryOnOrderPlacedHandler : IEventHandler<OrderPlacedIntegrationEvent>
{
    public bool Handled { get; private set; }

    public ValueTask HandleAsync(OrderPlacedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        Handled = true;
        Console.WriteLine($"      [UpdateInventoryOnOrderPlacedHandler] Reserving inventory for Order '{eventInstance.OrderId}'.");
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Handles payment processing for a placed order.
/// Successful handler for non-failing path demos.
/// </summary>
public sealed class PaymentProcessingHandler : IEventHandler<OrderPlacedIntegrationEvent>
{
    public ValueTask HandleAsync(OrderPlacedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        Console.WriteLine($"      [PaymentProcessingHandler] Payment settled for Order '{eventInstance.OrderId}'.");
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Always-failing handler for error handling and resilience policy demonstrations.
/// Used in Level 6 (ErrorHandlingPolicy.AggregateAndContinue).
/// </summary>
public sealed class FailingPaymentProcessingHandler : IEventHandler<OrderPlacedIntegrationEvent>
{
    public ValueTask HandleAsync(OrderPlacedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        Console.WriteLine($"      [FailingPaymentProcessingHandler] [SIMULATED FAULT] Gateway timeout on order '{eventInstance.OrderId}'!");
        throw new InvalidOperationException($"Payment gateway timeout for Order '{eventInstance.OrderId}'.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Event Handlers — OrderShippedIntegrationEvent
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Sends a shipment notification when an order is shipped.
/// Demonstrates a second distinct handler for a second event type.
/// </summary>
public sealed class SendShipmentNotificationHandler : IEventHandler<OrderShippedIntegrationEvent>
{
    public bool Handled { get; private set; }

    public ValueTask HandleAsync(OrderShippedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        Handled = true;
        Console.WriteLine($"      [SendShipmentNotificationHandler] Shipment notification sent for Order '{eventInstance.OrderId}' | Tracking: {eventInstance.TrackingNumber} ({eventInstance.CarrierCode}).");
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Updates fulfillment status when an order is shipped.
/// </summary>
public sealed class UpdateFulfillmentStatusHandler : IEventHandler<OrderShippedIntegrationEvent>
{
    public bool Handled { get; private set; }

    public ValueTask HandleAsync(OrderShippedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        Handled = true;
        Console.WriteLine($"      [UpdateFulfillmentStatusHandler] Fulfillment status updated for Order '{eventInstance.OrderId}'.");
        return ValueTask.CompletedTask;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Event Handlers — CustomerRegisteredIntegrationEvent
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Sends a welcome email when a customer registers.
/// </summary>
public sealed class SendWelcomeEmailHandler : IEventHandler<CustomerRegisteredIntegrationEvent>
{
    public bool Handled { get; private set; }

    public ValueTask HandleAsync(CustomerRegisteredIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        Handled = true;
        Console.WriteLine($"      [SendWelcomeEmailHandler] Welcome email sent to '{eventInstance.Email}'.");
        return ValueTask.CompletedTask;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ThrowOnUnregisteredEvent demo handler
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Minimal domain event for demonstrating ThrowOnUnregisteredEvent behavior.
/// No handlers are registered for this type — used to trigger the option.
/// </summary>
public sealed record UnhandledAuditEvent(
    EventId Id,
    string Action,
    DateTimeOffset OccurredAt) : IDomainEvent;

// ─────────────────────────────────────────────────────────────────────────────
// Cancellation demo handler
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Handler that respects CancellationToken, demonstrating proper cancellation propagation.
/// </summary>
public sealed class CancellationAwareHandler : IEventHandler<OrderPlacedIntegrationEvent>
{
    public bool Handled { get; private set; }
    public bool WasCancelled { get; private set; }

    public async ValueTask HandleAsync(OrderPlacedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        cancellationToken.ThrowIfCancellationRequested();

        // Simulate async work with cancellation support
        await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        Handled = true;
        Console.WriteLine($"      [CancellationAwareHandler] Processed order '{eventInstance.OrderId}' with cancellation support.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Reentrancy demo handler
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Handler that re-publishes events to demonstrate reentrancy depth tracking.
/// Used to demonstrate EventBusOptions.MaxReentrancyDepth.
/// NOTE: This handler is intentionally wired with a separate scoped bus — it does NOT
/// recursively call the same bus instance in the same scope.
/// </summary>
public sealed class ReentrancyTrackingHandler : IEventHandler<OrderPlacedIntegrationEvent>
{
    private readonly List<Guid> _processedOrders = new();
    public IReadOnlyList<Guid> ProcessedOrders => _processedOrders;

    public ValueTask HandleAsync(OrderPlacedIntegrationEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        _processedOrders.Add(eventInstance.OrderId);
        Console.WriteLine($"      [ReentrancyTrackingHandler] Processed Order '{eventInstance.OrderId}' (depth tracking active).");
        return ValueTask.CompletedTask;
    }
}
