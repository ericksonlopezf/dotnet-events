// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Contracts;

using EricksonLopez.Events.Envelopes;

/// <summary>
/// Provides extension methods for <see cref="IEventPublisher"/>.
/// </summary>
public static class EventPublisherExtensions
{
    /// <summary>
    /// Publishes an event wrapped in an <see cref="EventEnvelope{TEvent}"/> to registered subscribers asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event contained in the envelope.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="envelope">The envelope wrapping the event payload to publish.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="publisher"/> or <paramref name="envelope"/> is <see langword="null"/></exception>
    public static ValueTask PublishAsync<TEvent>(
        this IEventPublisher publisher,
        EventEnvelope<TEvent> envelope,
        CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(envelope);

        return publisher.PublishAsync(envelope.Payload, cancellationToken);
    }
}
