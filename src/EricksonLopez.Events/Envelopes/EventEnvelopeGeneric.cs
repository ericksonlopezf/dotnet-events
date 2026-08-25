// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;
using EricksonLopez.Events.Registry;

namespace EricksonLopez.Events.Envelopes;

/// <summary>
/// Represents an immutable envelope wrapping an event payload along with ambient metadata and transport headers.
/// </summary>
/// <remarks>
/// <para>Defaults are resolved at construction time:</para>
/// <list type="bullet">
/// <item><description>If <c>id</c> is empty, the envelope inherits <see cref="Contracts.IEvent.Id"/> from the payload.</description></item>
/// <item><description>If <c>type</c> is empty, the semantic event type is resolved from <see cref="Registry.StaticEventTypeRegistry"/>.</description></item>
/// <item><description>If <c>version</c> is the default, the schema version is resolved from <see cref="Registry.StaticEventTypeRegistry"/>.</description></item>
/// <item><description>If <c>occurredAt</c> is the default, the timestamp is inherited from the payload.</description></item>
/// </list>
/// </remarks>
/// <typeparam name="TEvent">The type of the event payload.</typeparam>
public sealed record EventEnvelope<TEvent> : IEventEnvelope where TEvent : IEvent
{
    /// <inheritdoc />
    public EventId Id { get; init; }

    /// <inheritdoc />
    public EventType Type { get; init; }

    /// <inheritdoc />
    public EventVersion Version { get; init; }

    /// <inheritdoc />
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>Gets the strongly-typed event payload carried within this envelope.</summary>
    public TEvent Payload { get; init; }

    /// <inheritdoc />
    public EventMetadata Metadata { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventEnvelope{TEvent}"/> class with the specified payload and envelope metadata.
    /// </summary>
    /// <param name="id">The unique identifier of the event.</param>
    /// <param name="type">The semantic event type.</param>
    /// <param name="version">The schema version of the event.</param>
    /// <param name="occurredAt">The timestamp when the event occurred.</param>
    /// <param name="payload">The event payload instance.</param>
    /// <param name="metadata">The optional ambient event metadata.</param>
    /// <exception cref="ArgumentNullException"><paramref name="payload"/> is <see langword="null"/></exception>
    public EventEnvelope(
        EventId id,
        EventType type,
        EventVersion version,
        DateTimeOffset occurredAt,
        TEvent payload,
        EventMetadata? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(payload);

        Id = id.IsEmpty ? payload.Id : id;
        Type = type.IsEmpty ? StaticEventTypeRegistry.GetEventType<TEvent>() : type;
        Version = version == default ? StaticEventTypeRegistry.GetVersion<TEvent>() : version;
        OccurredAt = occurredAt == default ? payload.OccurredAt : occurredAt;
        Payload = payload;
        Metadata = metadata ?? EventMetadata.Empty;
    }

    /// <inheritdoc />
    object IEventEnvelope.GetPayload() => Payload;
}
