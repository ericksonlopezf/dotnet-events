// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.Registry;

using System.Diagnostics.CodeAnalysis;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;

/// <summary>
/// Defines a registry for resolving event type metadata and descriptors.
/// </summary>
public interface IEventTypeRegistry
{
    /// <summary>
    /// Attempts to retrieve the descriptor associated with the specified semantic event type.
    /// </summary>
    /// <param name="eventType">The semantic event type identifier.</param>
    /// <param name="descriptor">When this method returns, contains the descriptor if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the descriptor was found; otherwise, <see langword="false"/>.</returns>
    bool TryGetDescriptor(EventType eventType, [NotNullWhen(true)] out EventTypeDescriptor? descriptor);

    /// <summary>
    /// Attempts to retrieve the descriptor associated with the specified semantic event type and contract schema version.
    /// </summary>
    /// <param name="eventType">The semantic event type identifier.</param>
    /// <param name="version">The schema contract version.</param>
    /// <param name="descriptor">When this method returns, contains the descriptor if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the descriptor was found; otherwise, <see langword="false"/>.</returns>
    bool TryGetDescriptor(EventType eventType, EventVersion version, [NotNullWhen(true)] out EventTypeDescriptor? descriptor);

    /// <summary>
    /// Attempts to retrieve the descriptor associated with the specified CLR event type.
    /// </summary>
    /// <param name="clrType">The CLR <see cref="Type"/> of the event.</param>
    /// <param name="descriptor">When this method returns, contains the descriptor if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the descriptor was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="clrType"/> is <see langword="null"/></exception>
    bool TryGetDescriptor(Type clrType, [NotNullWhen(true)] out EventTypeDescriptor? descriptor);

    /// <summary>
    /// Attempts to retrieve the descriptor associated with the generic event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="descriptor">When this method returns, contains the descriptor if found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the descriptor was found; otherwise, <see langword="false"/>.</returns>
    bool TryGetDescriptor<TEvent>([NotNullWhen(true)] out EventTypeDescriptor? descriptor) where TEvent : IEvent;

    /// <summary>
    /// Retrieves all registered event type descriptors.
    /// </summary>
    /// <returns>A read-only collection containing all registered <see cref="EventTypeDescriptor"/> instances.</returns>
    IReadOnlyCollection<EventTypeDescriptor> GetAllDescriptors();
}


