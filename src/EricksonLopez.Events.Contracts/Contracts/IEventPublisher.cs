// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Contracts;

/// <summary>
/// Defines a contract for publishing events within the application.
/// </summary>
public interface IEventPublisher
{
    /// <summary>Publishes an event to all registered subscribers asynchronously.</summary>
    /// <typeparam name="TEvent">The concrete event type to publish. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="eventInstance">The event instance to publish. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous publish operation.</returns>
    ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : IEvent;

    /// <summary>Publishes an event wrapped in an envelope carrying ambient metadata to all registered subscribers asynchronously.</summary>
    /// <typeparam name="TEvent">The concrete event type to publish. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="envelope">The event envelope carrying payload and ambient metadata. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous publish operation.</returns>
    async ValueTask PublishEnvelopeAsync<TEvent>(Envelopes.IEventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(envelope);
        using (Context.EventContext.SetCurrent(envelope))
        {
            await PublishAsync(envelope.Payload, cancellationToken).ConfigureAwait(false);
        }
    }
}




