// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Registry;

namespace EricksonLopez.Events.Envelopes;

/// <summary>
/// Provides factory methods for constructing <see cref="EventEnvelope{TEvent}"/> instances.
/// </summary>
public static class EventEnvelope
{
    /// <summary>
    /// Creates a new <see cref="EventEnvelope{TEvent}"/> wrapping the specified event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event payload.</typeparam>
    /// <param name="event">The event payload to wrap.</param>
    /// <param name="metadata">The optional ambient metadata.</param>
    /// <param name="type">The optional explicit event type override.</param>
    /// <param name="version">The optional explicit event version override.</param>
    /// <returns>A new <see cref="EventEnvelope{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/></exception>
    public static EventEnvelope<TEvent> Create<TEvent>(
        TEvent @event,
        EventMetadata? metadata = null,
        EventType? type = null,
        EventVersion? version = null) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        var descriptor = StaticEventTypeRegistry.GetDescriptor<TEvent>();
        var resolvedType = type ?? descriptor.EventType;
        var resolvedVersion = version ?? descriptor.Version;

        var meta = metadata ?? EventMetadata.Empty;
        if (string.IsNullOrEmpty(meta.Source) && !string.IsNullOrEmpty(descriptor.Source))
        {
            meta = meta with { Source = descriptor.Source };
        }

        return new EventEnvelope<TEvent>(
            @event.Id,
            resolvedType,
            resolvedVersion,
            @event.OccurredAt,
            @event,
            meta);
    }

    /// <summary>
    /// Creates a new <see cref="EventEnvelope{TEvent}"/> with explicit correlation, causation, and tenant identifiers.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event payload.</typeparam>
    /// <param name="event">The event payload to wrap.</param>
    /// <param name="correlationId">The optional correlation identifier.</param>
    /// <param name="causationId">The optional causation identifier.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="source">The optional originating source identifier.</param>
    /// <returns>A new <see cref="EventEnvelope{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/></exception>
    public static EventEnvelope<TEvent> Wrap<TEvent>(
        TEvent @event,
        CorrelationId? correlationId = null,
        CausationId? causationId = null,
        TenantId? tenantId = null,
        string? source = null) where TEvent : IEvent
    {
        var metadata = EventMetadata.Create(
            correlationId,
            causationId,
            tenantId,
            source);

        return Create(@event, metadata);
    }
}
