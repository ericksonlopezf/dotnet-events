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
}


