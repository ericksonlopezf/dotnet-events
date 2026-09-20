// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Contracts;

/// <summary>
/// Defines a contract for registering and managing in-memory event handlers.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>Registers a handler instance to receive events of the specified type.</summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="handler">The handler instance to register. Must not be <see langword="null"/>.</param>
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;

    /// <summary>Removes a previously registered handler from receiving events of the specified type.</summary>
    /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
    /// <param name="handler">The handler instance to remove. Must not be <see langword="null"/>.</param>
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;

    /// <summary>Registers an envelope handler instance to receive event envelopes of the specified type.</summary>
    /// <typeparam name="TEvent">The event payload type to subscribe to.</typeparam>
    /// <param name="handler">The envelope handler instance to register. Must not be <see langword="null"/>.</param>
    void Subscribe<TEvent>(IEnvelopeEventHandler<TEvent> handler) where TEvent : IEvent => throw new NotSupportedException();

    /// <summary>Removes a previously registered envelope handler from receiving event envelopes of the specified type.</summary>
    /// <typeparam name="TEvent">The event payload type to unsubscribe from.</typeparam>
    /// <param name="handler">The envelope handler instance to remove. Must not be <see langword="null"/>.</param>
    void Unsubscribe<TEvent>(IEnvelopeEventHandler<TEvent> handler) where TEvent : IEvent => throw new NotSupportedException();
}


