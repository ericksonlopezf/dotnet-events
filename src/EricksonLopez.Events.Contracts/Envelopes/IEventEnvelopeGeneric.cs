// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Envelopes;

/// <summary>
/// Defines a strongly-typed covariant abstraction for an event envelope carrying a specific event payload and ambient metadata.
/// </summary>
/// <typeparam name="TEvent">The type of the event payload.</typeparam>
public interface IEventEnvelope<out TEvent> : IEventEnvelope where TEvent : Contracts.IEvent
{
    /// <summary>
    /// Gets the strongly-typed event payload carried within this envelope.
    /// </summary>
    TEvent Payload { get; }
}
