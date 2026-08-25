// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Events.Bus.Registry;

namespace EricksonLopez.Events.Bus.Extensions;

/// <summary>
/// Internal token used to transfer handler registrations into the singleton <see cref="HandlerRegistry"/>.
/// </summary>
internal sealed class HandlerRegistrationToken
{
    public Type EventType { get; }
    public HandlerDescriptor Descriptor { get; }

    public HandlerRegistrationToken(Type eventType, HandlerDescriptor descriptor)
    {
        EventType = eventType;
        Descriptor = descriptor;
    }
}
