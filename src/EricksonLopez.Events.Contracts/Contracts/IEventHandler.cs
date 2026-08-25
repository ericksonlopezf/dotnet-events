// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Contracts;

using System.Diagnostics.CodeAnalysis;

/// <summary>Defines a strongly typed asynchronous handler for a specific event type.</summary>
/// <typeparam name="TEvent">The type of event this handler processes. Must implement <see cref="IEvent"/>.</typeparam>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "IEventHandler is the canonical domain event handler contract in DDD architectures.")]
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>Handles the specified event asynchronously.</summary>
    /// <param name="eventInstance">The event instance to process. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous handling operation.</returns>
    ValueTask HandleAsync(TEvent eventInstance, CancellationToken cancellationToken = default);
}




