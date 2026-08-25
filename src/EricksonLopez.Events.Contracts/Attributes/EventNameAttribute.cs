// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Attributes;

using EricksonLopez.Events.Identifiers;

/// <summary>
/// Specifies the explicit, transport-stable semantic name for an event type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EventNameAttribute : Attribute
{
    /// <summary>
    /// Gets the semantic event type name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The semantic event type name (e.g., "orders.order-created").</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    public EventNameAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    /// <summary>
    /// Converts the attribute name to a strongly typed <see cref="EventType"/>.
    /// </summary>
    /// <returns>A new <see cref="EventType"/> initialized with the configured name.</returns>
    public EventType AsEventType() => new(Name);
}


