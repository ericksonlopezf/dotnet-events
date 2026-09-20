// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus.Execution;

using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;
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

        var scopeFactory = options.ScopePolicy == HandlerScopePolicy.CreatePerHandler
            ? serviceProvider.GetService(typeof(IServiceScopeFactory)) as IServiceScopeFactory
            : null;

        var fallbackCache = scopeFactory == null && handlers.Count > 1
            ? new Dictionary<Type, object?>(handlers.Count)
            : null;

        for (int i = 0; i < handlers.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var descriptor = handlers[i];
            if (Context.EventContext.IsHandlerCompleted(eventInstance.Id, descriptor.HandlerType))
            {
                continue;
            }

            IServiceScope? scope = scopeFactory?.CreateScope();

            try
            {
                var currentProvider = scope?.ServiceProvider ?? serviceProvider;
                var handlerInstance = HandlerResolutionHelper.ResolveHandler<TEvent>(currentProvider, descriptor, fallbackCache);

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
                    var task = descriptor.TypedInvoker is HandlerInvoker<TEvent> typedInvoker
                        ? typedInvoker(handlerInstance, eventInstance, cancellationToken)
                        : descriptor.Invoker(handlerInstance, eventInstance, cancellationToken);

                    if (!task.IsCompletedSuccessfully)
                    {
                        await task.ConfigureAwait(false);
                    }

                    Context.EventContext.MarkHandlerCompleted(eventInstance.Id, descriptor.HandlerType);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (options.ErrorPolicy == ErrorHandlingPolicy.FailFast)
                    {
                        throw;
                    }

                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }
            finally
            {
                scope?.Dispose();
            }
        }

        if (exceptions != null)
        {
            throw new EventDispatchException(typeof(TEvent), exceptions);
        }
    }
}
