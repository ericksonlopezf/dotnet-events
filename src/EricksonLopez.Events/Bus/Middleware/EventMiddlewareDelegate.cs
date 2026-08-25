// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.Events.Bus.Middleware;

/// <summary>
/// Encapsulates an asynchronous operation that processes an event within the middleware execution pipeline.
/// </summary>
/// <typeparam name="TEvent">The type of the event being processed.</typeparam>
/// <param name="eventInstance">The event instance being processed.</param>
/// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
/// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "EventMiddlewareDelegate is the standard pipeline delegate convention.")]
public delegate ValueTask EventMiddlewareDelegate<in TEvent>(TEvent eventInstance, CancellationToken cancellationToken)
    where TEvent : IEvent;
