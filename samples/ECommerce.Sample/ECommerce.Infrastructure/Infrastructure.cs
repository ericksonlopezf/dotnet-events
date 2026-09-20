// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure;

using ECommerce.Application;
using ECommerce.Domain;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Serialization.SystemTextJson.Converters;

// ─────────────────────────────────────────────────────────────────────────────
// Native AOT JSON Context
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// AOT-compatible JSON serializer context for all e-commerce event types.
/// All event types and their envelopes must be registered here for zero-reflection serialization.
/// This satisfies Native AOT's requirement of compile-time type knowledge.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = false,
    Converters = [
        typeof(EventIdJsonConverter),
        typeof(EventTypeJsonConverter),
        typeof(EventVersionJsonConverter),
        typeof(CorrelationIdJsonConverter),
        typeof(CausationIdJsonConverter),
        typeof(TenantIdJsonConverter),
        typeof(EventMetadataJsonConverter)
    ])]
[JsonSerializable(typeof(EventId))]
[JsonSerializable(typeof(EricksonLopez.Events.Identifiers.EventType))]
[JsonSerializable(typeof(EventVersion))]
[JsonSerializable(typeof(CorrelationId))]
[JsonSerializable(typeof(CausationId))]
[JsonSerializable(typeof(TenantId))]
[JsonSerializable(typeof(EventMetadata))]
// ─── Integration Event Payloads ───────────────────────────────────────────
[JsonSerializable(typeof(OrderPlacedIntegrationEvent))]
[JsonSerializable(typeof(OrderShippedIntegrationEvent))]
[JsonSerializable(typeof(CustomerRegisteredIntegrationEvent))]
// ─── EventEnvelope<T> wrappers ─────────────────────────────────────────────
[JsonSerializable(typeof(EventEnvelope<OrderPlacedIntegrationEvent>))]
[JsonSerializable(typeof(EventEnvelope<OrderShippedIntegrationEvent>))]
[JsonSerializable(typeof(EventEnvelope<CustomerRegisteredIntegrationEvent>))]
internal sealed partial class ECommerceJsonContext : JsonSerializerContext
{
}

// ─────────────────────────────────────────────────────────────────────────────
// Showcase JSON Serializer (AOT facade)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Provides AOT-compatible JSON serialization/deserialization for showcase event envelopes.
/// Uses <see cref="ECommerceJsonContext"/> for zero-reflection operation.
/// </summary>
public static class ECommerceJsonSerializer
{
    public static string Serialize<TEvent>(EventEnvelope<TEvent> envelope) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(envelope);
        if (envelope is EventEnvelope<OrderPlacedIntegrationEvent> orderEnv)
        {
            return JsonSerializer.Serialize(orderEnv, ECommerceJsonContext.Default.EventEnvelopeOrderPlacedIntegrationEvent);
        }

        if (envelope is EventEnvelope<OrderShippedIntegrationEvent> shippedEnv)
        {
            return JsonSerializer.Serialize(shippedEnv, ECommerceJsonContext.Default.EventEnvelopeOrderShippedIntegrationEvent);
        }

        if (envelope is EventEnvelope<CustomerRegisteredIntegrationEvent> customerEnv)
        {
            return JsonSerializer.Serialize(customerEnv, ECommerceJsonContext.Default.EventEnvelopeCustomerRegisteredIntegrationEvent);
        }

        throw new NotSupportedException($"Type {typeof(TEvent).Name} is not registered in ECommerceJsonContext.");
    }

    public static EventEnvelope<OrderPlacedIntegrationEvent>? DeserializeOrderPlacedEnvelope(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize(json, ECommerceJsonContext.Default.EventEnvelopeOrderPlacedIntegrationEvent);
    }

    public static EventEnvelope<OrderShippedIntegrationEvent>? DeserializeOrderShippedEnvelope(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize(json, ECommerceJsonContext.Default.EventEnvelopeOrderShippedIntegrationEvent);
    }

    public static EventEnvelope<CustomerRegisteredIntegrationEvent>? DeserializeCustomerRegisteredEnvelope(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize(json, ECommerceJsonContext.Default.EventEnvelopeCustomerRegisteredIntegrationEvent);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Outbox Service (showcase-local, not EricksonLopez.Outbox)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Represents a persisted outbox record.
/// </summary>
public sealed record OutboxRecord(
    Guid Id,
    string EventType,
    uint Version,
    string PayloadJson,
    DateTimeOffset CreatedAtUtc,
    bool Processed);

/// <summary>
/// In-memory outbox service for the showcase.
/// Demonstrates the event-to-envelope-to-outbox flow used by <see cref="OrderApplicationService"/>.
/// </summary>
public sealed class InMemoryOutboxService : IOutboxService
{
    private readonly ConcurrentQueue<OutboxRecord> _queue = new();

    public IReadOnlyCollection<OutboxRecord> Records => _queue.ToArray();

    public ValueTask EnqueueAsync<TEvent>(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(envelope);

        string json = ECommerceJsonSerializer.Serialize(envelope);

        var record = new OutboxRecord(
            envelope.Id.Value,
            envelope.Type.Value,
            envelope.Version.Value,
            json,
            DateTimeOffset.UtcNow,
            false);

        _queue.Enqueue(record);
        return ValueTask.CompletedTask;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Middleware Implementations
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Logging middleware that wraps every event dispatch with structured log output.
/// Demonstrates IEventMiddleware's before/after pipeline interception pattern.
/// </summary>
public sealed class LoggingEventMiddleware : IEventMiddleware
{
    public async ValueTask InvokeAsync<TEvent>(
        TEvent eventInstance,
        EventMiddlewareDelegate<TEvent> nextHandler,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        ArgumentNullException.ThrowIfNull(nextHandler);

        Console.WriteLine($"    [Middleware] [LOG-START] Dispatching event '{typeof(TEvent).Name}' (Id: {eventInstance.Id})...");
        await nextHandler(eventInstance, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"    [Middleware] [LOG-END] Successfully dispatched '{typeof(TEvent).Name}'.");
    }
}

/// <summary>
/// Performance metrics middleware that measures and reports handler execution duration.
/// Demonstrates IEventMiddleware's cross-cutting concern pattern using Stopwatch.
/// </summary>
public sealed class PerformanceMetricsMiddleware : IEventMiddleware
{
    public async ValueTask InvokeAsync<TEvent>(
        TEvent eventInstance,
        EventMiddlewareDelegate<TEvent> nextHandler,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        ArgumentNullException.ThrowIfNull(nextHandler);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await nextHandler(eventInstance, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine($"    [Middleware] [PERF] Event '{typeof(TEvent).Name}' processed in {stopwatch.Elapsed.TotalMilliseconds:F3} ms.");
        }
    }
}

/// <summary>
/// Validation middleware that inspects the event instance before dispatch.
/// Demonstrates middleware that can short-circuit the pipeline by not calling next.
/// </summary>
public sealed class ValidationMiddleware : IEventMiddleware
{
    private readonly List<string> _validationLog = new();
    public IReadOnlyList<string> ValidationLog => _validationLog;

    public async ValueTask InvokeAsync<TEvent>(
        TEvent eventInstance,
        EventMiddlewareDelegate<TEvent> nextHandler,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        ArgumentNullException.ThrowIfNull(nextHandler);

        _validationLog.Add($"Validated: {typeof(TEvent).Name} (Id: {eventInstance.Id})");
        Console.WriteLine($"    [Middleware] [VALIDATE] Event '{typeof(TEvent).Name}' passed validation.");
        await nextHandler(eventInstance, cancellationToken).ConfigureAwait(false);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Custom IExecutionStrategy Implementation
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A custom execution strategy that executes handlers sequentially and logs each step.
/// Demonstrates implementing IExecutionStrategy for full control over handler execution.
/// </summary>
public sealed class VerboseSequentialExecutionStrategy : EricksonLopez.Events.Bus.Execution.IExecutionStrategy
{
    private int _totalHandlersExecuted;
    public int TotalHandlersExecuted => _totalHandlersExecuted;

    public async ValueTask ExecuteAsync<TEvent>(
        IReadOnlyList<HandlerDescriptor> handlers,
        TEvent eventInstance,
        IServiceProvider serviceProvider,
        EventBusOptions options,
        CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(eventInstance);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        Console.WriteLine($"    [CustomStrategy] Executing {handlers.Count} handler(s) for '{typeof(TEvent).Name}' sequentially.");

        var exceptions = new List<Exception>();

        for (int i = 0; i < handlers.Count; i++)
        {
            var descriptor = handlers[i];
            var handler = serviceProvider.GetService(descriptor.ServiceType);
            if (handler is null)
            {
                continue;
            }

            Console.WriteLine($"    [CustomStrategy]   -> Handler [{i + 1}/{handlers.Count}]: {descriptor.HandlerType.Name}");

            try
            {
                await descriptor.Invoker(handler, eventInstance!, cancellationToken).ConfigureAwait(false);
                Interlocked.Increment(ref _totalHandlersExecuted);
            }
            catch (Exception ex) when (options.ErrorPolicy == ErrorHandlingPolicy.AggregateAndContinue)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new EricksonLopez.Events.Bus.Exceptions.EventDispatchException(typeof(TEvent), exceptions);
        }
    }
}
