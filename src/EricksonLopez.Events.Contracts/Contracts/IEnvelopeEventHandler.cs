// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Envelopes;

namespace EricksonLopez.Events.Contracts;

/// <summary>
/// Defines a handler for processing events wrapped in an <see cref="IEventEnvelope{TEvent}"/> carrying ambient metadata and transport headers.
/// </summary>
/// <typeparam name="TEvent">The type of the event contained in the envelope.</typeparam>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "IEnvelopeEventHandler is the canonical envelope event handler contract in DDD architectures.")]
public interface IEnvelopeEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the event envelope asynchronously.
    /// </summary>
    /// <param name="envelope">The event envelope carrying payload and metadata.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask HandleAsync(IEventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default);
}
