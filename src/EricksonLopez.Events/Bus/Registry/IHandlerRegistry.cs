// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.Bus.Registry;


/// <summary>
/// Defines a registry that provides read-only access to registered event handler descriptors.
/// </summary>
public interface IHandlerRegistry
{
    /// <summary>
    /// Retrieves registered handler descriptors for the specified event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <returns>A read-only list of handler descriptors, or an empty list if none are registered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> is <see langword="null"/></exception>
    IReadOnlyList<HandlerDescriptor> GetHandlers(Type eventType);

    /// <summary>
    /// Determines whether any handlers are registered for the specified event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <returns><see langword="true"/> if at least one handler is registered; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> is <see langword="null"/></exception>
    bool HasHandlers(Type eventType);
}


