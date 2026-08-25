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
/// Executes registered handlers for an event one at a time in registration order,
/// applying the configured <see cref="ErrorHandlingPolicy"/> upon handler failures.
/// </summary>
public sealed class SequentialExecutionStrategy : IExecutionStrategy
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

        List<Exception>? exceptions = null;
        ILogger? logger = null;
        bool loggerResolved = false;

        for (int i = 0; i < handlers.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var descriptor = handlers[i];
            var handlerInstance = serviceProvider.GetService(descriptor.HandlerType);

            if (handlerInstance == null)
            {
                if (!loggerResolved)
                {
                    logger = serviceProvider.GetService(typeof(ILogger<SequentialExecutionStrategy>)) as ILogger;
                    loggerResolved = true;
                }

                logger?.LogWarning(
                    "Handler '{HandlerType}' registered for event '{EventType}' could not be resolved from service provider (GetService returned null). Skipping handler.",
                    descriptor.HandlerType.FullName,
                    typeof(TEvent).FullName);

                continue;
            }

            try
            {
                var task = descriptor.Invoker(handlerInstance, eventInstance, cancellationToken);
                if (!task.IsCompletedSuccessfully)
                {
                    await task.ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (options.ErrorPolicy == ErrorHandlingPolicy.FailFast)
                {
                    throw;
                }

                exceptions ??= new List<Exception>();
                exceptions.Add(ex);
            }
        }

        if (exceptions != null)
        {
            throw new EventDispatchException(typeof(TEvent), exceptions);
        }
    }
}
