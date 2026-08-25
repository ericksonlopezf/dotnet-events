// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Registry;

using EricksonLopez.Events.Identifiers;

/// <summary>
/// Represents metadata describing a registered event type.
/// </summary>
/// <param name="ClrType">The CLR <see cref="Type"/> of the event.</param>
/// <param name="EventType">The strongly-typed semantic event type.</param>
/// <param name="Version">The schema contract version.</param>
/// <param name="Source">The optional default producer source URI or identifier.</param>
public sealed record EventTypeDescriptor(
    Type ClrType,
    EventType EventType,
    EventVersion Version,
    string? Source = null)
{
    /// <summary>
    /// Creates an <see cref="EventTypeDescriptor"/> for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="eventType">The semantic event type.</param>
    /// <param name="version">The optional contract version. Defaults to <see cref="EventVersion.V1"/>.</param>
    /// <param name="source">The optional event producer source.</param>
    /// <returns>A new <see cref="EventTypeDescriptor"/> instance.</returns>
    public static EventTypeDescriptor For<TEvent>(
        EventType eventType,
        EventVersion? version = null,
        string? source = null) =>
        new(typeof(TEvent), eventType, version ?? EventVersion.V1, source);
}


