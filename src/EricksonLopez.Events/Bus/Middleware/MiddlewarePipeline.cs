// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;

namespace EricksonLopez.Events.Bus.Middleware;

using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;

/// <summary>
/// Provides methods for constructing middleware execution pipelines for event dispatch.
/// </summary>
public static class MiddlewarePipeline
{
    /// <summary>
    /// Chains a collection of middlewares around a terminal execution delegate.
    /// </summary>
    /// <typeparam name="TEvent">The type of event being dispatched.</typeparam>
    /// <param name="middlewares">The collection of middleware instances to chain.</param>
    /// <param name="terminal">The terminal execution delegate that dispatches to handlers.</param>
    /// <returns>A composed <see cref="EventMiddlewareDelegate{TEvent}"/> pipeline delegate.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="terminal"/> is <see langword="null"/></exception>
    public static EventMiddlewareDelegate<TEvent> Build<TEvent>(
        IReadOnlyList<IEventMiddleware> middlewares,
        EventMiddlewareDelegate<TEvent> terminal)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(terminal);

        if (middlewares == null || middlewares.Count == 0)
        {
            return terminal;
        }

        EventMiddlewareDelegate<TEvent> current = terminal;
        for (int i = middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = middlewares[i];
            var next = current;
            current = (@event, ct) => middleware.InvokeAsync(@event, next, ct);
        }

        return current;
    }
}



