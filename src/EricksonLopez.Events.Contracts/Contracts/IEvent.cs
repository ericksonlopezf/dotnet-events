// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Contracts;

using EricksonLopez.Events.Identifiers;

/// <summary>
/// Defines the core contract for all events within the application ecosystem.
/// </summary>
public interface IEvent
{
    /// <summary>Gets the unique identifier of this event instance.</summary>
    EventId Id { get; }

    /// <summary>Gets the UTC timestamp at which the event occurrence was recorded.</summary>
    DateTimeOffset OccurredAt { get; }
}


