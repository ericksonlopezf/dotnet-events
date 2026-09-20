// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus.Execution;

using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;

/// <summary>
/// Defines a strategy for executing event handlers during dispatch.
/// </summary>
public interface IExecutionStrategy
{
    /// <summary>
    /// Executes registered handlers for the specified event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to handle.</typeparam>
    /// <param name="handlers">The descriptors of registered handlers.</param>
    /// <param name="eventInstance">The event instance to dispatch.</param>
    /// <param name="serviceProvider">The service provider used to resolve handler dependencies.</param>
    /// <param name="options">The event bus configuration options.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask ExecuteAsync<TEvent>(
        IReadOnlyList<HandlerDescriptor> handlers,
        TEvent eventInstance,
        IServiceProvider serviceProvider,
        EventBusOptions options,
        CancellationToken cancellationToken)
        where TEvent : IEvent;
}



