// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Events.Bus.Registry;

namespace EricksonLopez.Events.Bus.Extensions;

/// <summary>
/// Token used to transfer handler registrations into the singleton <see cref="HandlerRegistry"/>.
/// </summary>
public sealed class HandlerRegistrationToken
{
    /// <summary>
    /// Gets the type of the event handled by this registration.
    /// </summary>
    public Type EventType { get; }

    /// <summary>
    /// Gets the handler descriptor containing dispatch metadata and static invoker delegate.
    /// </summary>
    public HandlerDescriptor Descriptor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerRegistrationToken"/> class.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="descriptor">The handler descriptor.</param>
    public HandlerRegistrationToken(Type eventType, HandlerDescriptor descriptor)
    {
        EventType = eventType;
        Descriptor = descriptor;
    }
}
