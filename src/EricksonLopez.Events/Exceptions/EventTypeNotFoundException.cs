// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Exceptions;

using EricksonLopez.Events.Identifiers;

/// <summary>
/// Represents errors that occur when an event type is not found in the registry.
/// </summary>
public sealed class EventTypeNotFoundException : Exception
{
    /// <summary>
    /// Gets the unresolved event type.
    /// </summary>
    public EventType EventType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventTypeNotFoundException"/> class for the specified event type.
    /// </summary>
    /// <param name="eventType">The unresolved event type identifier.</param>
    public EventTypeNotFoundException(EventType eventType)
        : base($"Event type '{eventType}' is not registered in the event registry.")
    {
        EventType = eventType;
    }
}
