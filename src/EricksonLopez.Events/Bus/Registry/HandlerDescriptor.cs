// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus.Registry;

/// <summary>
/// Represents a registered event handler and its invocation metadata.
/// </summary>
public sealed class HandlerDescriptor
{
    /// <summary>
    /// Gets the concrete implementation type of the handler.
    /// </summary>
    public Type HandlerType { get; }

    /// <summary>
    /// Gets the resolved service interface type.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Gets the invocation delegate that executes the handler asynchronously.
    /// </summary>
    public Func<object, object, CancellationToken, ValueTask> Invoker { get; }

    /// <summary>
    /// Gets the strongly typed invocation delegate, if available, avoiding boxing for value-type events.
    /// </summary>
    public object? TypedInvoker { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerDescriptor"/> class with the specified types, invocation delegate, and optional typed invoker.
    /// </summary>
    /// <param name="handlerType">The concrete handler implementation type.</param>
    /// <param name="serviceType">The service interface type.</param>
    /// <param name="invoker">The invoker delegate.</param>
    /// <param name="typedInvoker">The optional strongly typed invoker delegate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handlerType"/>, <paramref name="serviceType"/>, or <paramref name="invoker"/> is <see langword="null"/></exception>
    public HandlerDescriptor(
        Type handlerType,
        Type serviceType,
        Func<object, object, CancellationToken, ValueTask> invoker,
        object? typedInvoker = null)
    {
        HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
        TypedInvoker = typedInvoker;
    }
}
