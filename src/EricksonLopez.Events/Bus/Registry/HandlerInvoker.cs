// Copyright © Erickson Lopez. MIT License.
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus.Registry;

/// <summary>
/// Encapsulates a method that asynchronously invokes a handler instance with the given event payload.
/// </summary>
/// <typeparam name="TEvent">The type of event being handled.</typeparam>
/// <param name="handler">The resolved handler instance.</param>
/// <param name="event">The event instance to handle.</param>
/// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
/// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
public delegate ValueTask HandlerInvoker<in TEvent>(object handler, TEvent @event, CancellationToken cancellationToken);
