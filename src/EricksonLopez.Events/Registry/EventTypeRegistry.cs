// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;

namespace EricksonLopez.Events.Registry;

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

/// <summary>
/// Provides an immutable registry for resolving event type descriptors.
/// </summary>
public sealed class EventTypeRegistry : IEventTypeRegistry
{
    private readonly FrozenDictionary<EventType, EventTypeDescriptor> _byEventType;
    private readonly FrozenDictionary<Type, EventTypeDescriptor> _byClrType;
    private readonly EventTypeDescriptor[] _allDescriptors;

    /// <summary>
    /// Gets the empty default registry instance.
    /// </summary>
    public static readonly EventTypeRegistry Empty = new(Array.Empty<EventTypeDescriptor>());

    /// <summary>
    /// Initializes a new instance of the <see cref="EventTypeRegistry"/> class with the specified descriptors.
    /// </summary>
    /// <param name="descriptors">The collection of descriptors to register.</param>
    /// <exception cref="ArgumentNullException"><paramref name="descriptors"/> is <see langword="null"/></exception>
    public EventTypeRegistry(IEnumerable<EventTypeDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);

        var list = new List<EventTypeDescriptor>(descriptors);
        _allDescriptors = list.ToArray();

        var byTypeDict = new Dictionary<EventType, EventTypeDescriptor>();
        var byClrDict = new Dictionary<Type, EventTypeDescriptor>();

        foreach (var desc in list)
        {
            byTypeDict[desc.EventType] = desc;
            byClrDict[desc.ClrType] = desc;
        }

        _byEventType = byTypeDict.ToFrozenDictionary();
        _byClrType = byClrDict.ToFrozenDictionary();
    }

    /// <inheritdoc />
    public bool TryGetDescriptor(EventType eventType, [NotNullWhen(true)] out EventTypeDescriptor? descriptor) =>
        _byEventType.TryGetValue(eventType, out descriptor);

    /// <inheritdoc />
    public bool TryGetDescriptor(Type clrType, [NotNullWhen(true)] out EventTypeDescriptor? descriptor)
    {
        ArgumentNullException.ThrowIfNull(clrType);
        return _byClrType.TryGetValue(clrType, out descriptor);
    }

    /// <inheritdoc />
    public bool TryGetDescriptor<TEvent>([NotNullWhen(true)] out EventTypeDescriptor? descriptor)
        where TEvent : IEvent =>
        _byClrType.TryGetValue(typeof(TEvent), out descriptor);

    /// <inheritdoc />
    public IReadOnlyCollection<EventTypeDescriptor> GetAllDescriptors() => _allDescriptors;

    /// <summary>
    /// Creates a builder for constructing a new <see cref="EventTypeRegistry"/>.
    /// </summary>
    /// <returns>A new <see cref="EventTypeRegistryBuilder"/> instance.</returns>
    public static EventTypeRegistryBuilder CreateBuilder() => new();
}



