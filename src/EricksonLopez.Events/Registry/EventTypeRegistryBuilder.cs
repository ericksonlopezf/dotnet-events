// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

namespace EricksonLopez.Events.Registry;

/// <summary>
/// Provides a builder for constructing immutable <see cref="EventTypeRegistry"/> instances.
/// </summary>
public sealed class EventTypeRegistryBuilder
{
    private readonly List<EventTypeDescriptor> _descriptors = new();

    /// <summary>Registers an existing event type descriptor in the builder.</summary>
    /// <param name="descriptor">The descriptor to register.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="descriptor"/> is <see langword="null"/></exception>
    public EventTypeRegistryBuilder Register(EventTypeDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _descriptors.Add(descriptor);
        return this;
    }

    /// <summary>Registers an event type with the specified semantic name, version, and source.</summary>
    /// <typeparam name="TEvent">The CLR type of the event to register. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="eventTypeName">The semantic event type name (e.g., <c>orders.order-created</c>).</param>
    /// <param name="version">The schema version number. Must be greater than or equal to 1. The default is 1.</param>
    /// <param name="source">The optional originating source identifier or URI.</param>
    /// <returns>The current builder instance to support method chaining.</returns>
    public EventTypeRegistryBuilder Register<TEvent>(
        string eventTypeName,
        uint version = 1,
        string? source = null) where TEvent : IEvent
    {
        _descriptors.Add(EventTypeDescriptor.For<TEvent>(
            EventType.From(eventTypeName),
            EventVersion.From(version),
            source));
        return this;
    }

    /// <summary>Builds and returns the immutable <see cref="EventTypeRegistry"/> containing all registered descriptors.</summary>
    /// <returns>A new <see cref="EventTypeRegistry"/> containing all registered descriptors.</returns>
    public EventTypeRegistry Build() => new(_descriptors);
}
