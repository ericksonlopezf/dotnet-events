// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus.Execution;

using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Registry;
using Microsoft.Extensions.Logging;

/// <summary>
/// Executes all registered handlers for an event concurrently, collecting and aggregating any handler exceptions
/// into a single <see cref="EventDispatchException"/>.
/// </summary>
public sealed class ParallelExecutionStrategy : IExecutionStrategy
{
    /// <inheritdoc />
    [SuppressMessage("Usage", "CA1848:Use the LoggerMessage delegates", Justification = "Fallback warning logging for unresolved handlers")]
    public async ValueTask ExecuteAsync<TEvent>(
        IReadOnlyList<HandlerDescriptor> handlers,
        TEvent eventInstance,
        IServiceProvider serviceProvider,
        EventBusOptions options,
        CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(eventInstance);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(options);

        switch (handlers.Count)
        {
            case 0:
                return;

            case 1:
                var descriptor = handlers[0];
                var instance = serviceProvider.GetService(descriptor.HandlerType);
                if (instance != null)
                {
                    await descriptor.Invoker(instance, eventInstance, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var logger = serviceProvider.GetService(typeof(ILogger<ParallelExecutionStrategy>)) as ILogger;
                    logger?.LogWarning(
                        "Handler '{HandlerType}' registered for event '{EventType}' could not be resolved from service provider (GetService returned null). Skipping handler.",
                        descriptor.HandlerType.FullName,
                        typeof(TEvent).FullName);
                }
                return;

            default:
                var tasks = new Task[handlers.Count];
                ILogger? multiLogger = null;
                bool loggerResolved = false;

                for (int i = 0; i < handlers.Count; i++)
                {
                    var desc = handlers[i];
                    var inst = serviceProvider.GetService(desc.HandlerType);
                    if (inst != null)
                    {
                        tasks[i] = desc.Invoker(inst, eventInstance, cancellationToken).AsTask();
                    }
                    else
                    {
                        if (!loggerResolved)
                        {
                            multiLogger = serviceProvider.GetService(typeof(ILogger<ParallelExecutionStrategy>)) as ILogger;
                            loggerResolved = true;
                        }

                        multiLogger?.LogWarning(
                            "Handler '{HandlerType}' registered for event '{EventType}' could not be resolved from service provider (GetService returned null). Skipping handler.",
                            desc.HandlerType.FullName,
                            typeof(TEvent).FullName);

                        tasks[i] = Task.CompletedTask;
                    }
                }

                try
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch
                {
                    var exceptions = new List<Exception>();
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        if (tasks[i].Exception is { } ex)
                        {
                            exceptions.AddRange(ex.Flatten().InnerExceptions);
                        }
                    }

                    if (exceptions.Count > 0)
                    {
                        throw new EventDispatchException(typeof(TEvent), exceptions);
                    }

                    throw;
                }
                return;
        }
    }
}
