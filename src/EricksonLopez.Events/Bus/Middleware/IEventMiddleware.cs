// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus.Middleware;

using EricksonLopez.Events.Contracts;

/// <summary>
/// Defines a middleware component that intercepts event dispatch execution.
/// </summary>
public interface IEventMiddleware
{
    /// <summary>
    /// Executes the middleware processing logic around event publication.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event being dispatched.</typeparam>
    /// <param name="eventInstance">The event instance being dispatched.</param>
    /// <param name="nextHandler">The delegate representing the next middleware or terminal handler in the pipeline.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask InvokeAsync<TEvent>(
        TEvent eventInstance,
        EventMiddlewareDelegate<TEvent> nextHandler,
        CancellationToken cancellationToken)
        where TEvent : IEvent;
}




