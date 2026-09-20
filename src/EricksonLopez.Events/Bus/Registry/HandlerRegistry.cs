// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;

namespace EricksonLopez.Events.Bus.Registry;

using System.Collections.Concurrent;

/// <summary>
/// Provides a thread-safe registry for storing and resolving event handler descriptors.
/// </summary>
public sealed class HandlerRegistry : IHandlerRegistry
{
    private static readonly HandlerDescriptor[] EmptyHandlers = Array.Empty<HandlerDescriptor>();
    private readonly ConcurrentDictionary<Type, HandlerDescriptor[]> _handlers = new();

    /// <summary>
    /// Registers a handler descriptor for the specified event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="descriptor">The handler descriptor to register.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eventType"/> or <paramref name="descriptor"/> is <see langword="null"/></exception>
    public void Register(Type eventType, HandlerDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(descriptor);

        _handlers.AddOrUpdate(
            eventType,
            _ => new[] { descriptor },
            (_, existing) =>
            {
                for (int i = 0; i < existing.Length; i++)
                {
                    if (existing[i].HandlerType == descriptor.HandlerType &&
                        existing[i].ServiceType == descriptor.ServiceType &&
                        existing[i].Invoker == descriptor.Invoker)
                    {
                        return existing;
                    }
                }

                var updated = new HandlerDescriptor[existing.Length + 1];
                Array.Copy(existing, updated, existing.Length);
                updated[^1] = descriptor;
                return updated;
            });
    }

    /// <inheritdoc />
    public IReadOnlyList<HandlerDescriptor> GetHandlers(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        return _handlers.TryGetValue(eventType, out var list) ? list : EmptyHandlers;
    }

    /// <inheritdoc />
    public bool HasHandlers(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        return _handlers.ContainsKey(eventType);
    }
}



