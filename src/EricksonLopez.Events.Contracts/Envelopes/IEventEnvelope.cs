// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Envelopes;

using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

/// <summary>
/// Defines a non-generic abstraction for an event envelope carrying the event payload, ambient metadata, and transport headers.
/// </summary>
/// <remarks>
/// Use this interface when the concrete event type is not known at compile time, such as during serialization or outbox dispatching.
/// To access a strongly-typed payload, use <c>EventEnvelope&lt;TEvent&gt;</c> or cast the result of <see cref="GetPayload"/>.
/// </remarks>
public interface IEventEnvelope
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    EventId Id { get; }

    /// <summary>
    /// Gets the semantic event type.
    /// </summary>
    EventType Type { get; }

    /// <summary>
    /// Gets the schema version of the event contract.
    /// </summary>
    EventVersion Version { get; }

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Gets the ambient event metadata (correlation, causation, tenant, headers).
    /// </summary>
    EventMetadata Metadata { get; }

    /// <summary>Returns the untyped event payload for scenarios where the concrete event type is unknown at compile time.</summary>
    /// <returns>The event payload instance as <see cref="object"/>.</returns>
    object GetPayload();
}


