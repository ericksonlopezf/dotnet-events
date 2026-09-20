// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Envelopes;

namespace EricksonLopez.Events.Testing;

/// <summary>
/// Provides factory methods for building synthetic test events and envelopes.
/// </summary>
public static class EventTestBuilder
{
    /// <summary>
    /// Creates a fluent test builder for an <see cref="EventEnvelope{TEvent}"/>.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event payload.</typeparam>
    /// <param name="payload">The event payload.</param>
    /// <returns>A new <see cref="EventEnvelopeTestBuilder{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="payload"/> is <see langword="null"/></exception>
    public static EventEnvelopeTestBuilder<TEvent> For<TEvent>(TEvent payload) where TEvent : IEvent =>
        new(payload);
}
