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
/// Executes all registered handlers for an event concurrently, collecting and aggregating any handler exceptions
/// into a single <see cref="EventDispatchException"/>, or failing fast if configured.
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

        var shouldCreateScope = options.ScopePolicy == HandlerScopePolicy.CreatePerHandler ||
                                options.ScopePolicy == HandlerScopePolicy.Auto;

        if (!shouldCreateScope && handlers.Count > 1)
        {
            var logger = serviceProvider.GetService(typeof(ILogger<ParallelExecutionStrategy>)) as ILogger;
            logger?.LogWarning(
                "ParallelExecutionStrategy is executing with HandlerScopePolicy.ReuseAmbientScope for event '{EventType}'. Handlers will share the parent IServiceProvider concurrently. Ensure handlers and their dependencies (such as DbContext) are thread-safe.",
                typeof(TEvent).FullName);
        }

        var scopeFactory = shouldCreateScope
            ? serviceProvider.GetService(typeof(IServiceScopeFactory)) as IServiceScopeFactory
            : null;

        switch (handlers.Count)
        {
            case 0:
                return;

            case 1:
                var descriptor = handlers[0];
                if (Context.EventContext.IsHandlerCompleted(eventInstance.Id, descriptor.HandlerType))
                {
                    return;
                }

                IServiceScope? singleScope = scopeFactory?.CreateScope();
                try
                {
                    var provider = singleScope?.ServiceProvider ?? serviceProvider;
                    var instance = HandlerResolutionHelper.ResolveHandler<TEvent>(provider, descriptor);
                    if (instance != null)
                    {
                        var vt = descriptor.TypedInvoker is HandlerInvoker<TEvent> typedInvoker
                            ? typedInvoker(instance, eventInstance, cancellationToken)
                            : descriptor.Invoker(instance, eventInstance, cancellationToken);
                        if (!vt.IsCompletedSuccessfully)
                        {
                            await vt.ConfigureAwait(false);
                        }

                        Context.EventContext.MarkHandlerCompleted(eventInstance.Id, descriptor.HandlerType);
                    }
                    else
                    {
                        var logger = serviceProvider.GetService(typeof(ILogger<ParallelExecutionStrategy>)) as ILogger;
                        logger?.LogWarning(
                            "Handler '{HandlerType}' registered for event '{EventType}' could not be resolved from service provider (GetService returned null). Skipping handler.",
                            descriptor.HandlerType.FullName,
                            typeof(TEvent).FullName);
                    }
                }
                finally
                {
                    singleScope?.Dispose();
                }
                return;

            default:
                {
                    var tasks = new Task[handlers.Count];
                    ILogger? multiLogger = null;
                    bool loggerResolved = false;
                    object loggerSync = new();

                    using var linkedCts = options.ErrorPolicy == ErrorHandlingPolicy.FailFast
                        ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                        : null;

                    var effectiveToken = linkedCts?.Token ?? cancellationToken;

                    using var throttler = options.MaxDegreeOfParallelism > 0
                        ? new SemaphoreSlim(options.MaxDegreeOfParallelism, options.MaxDegreeOfParallelism)
                        : null;

                    object?[] preResolvedInstances = new object?[handlers.Count];
                    if (scopeFactory == null)
                    {
                        for (int i = 0; i < handlers.Count; i++)
                        {
                            preResolvedInstances[i] = HandlerResolutionHelper.ResolveHandler<TEvent>(serviceProvider, handlers[i]);
                        }
                    }

                    for (int i = 0; i < handlers.Count; i++)
                    {
                        var desc = handlers[i];
                        object? preResolvedInstance = scopeFactory == null ? preResolvedInstances[i] : null;

                        tasks[i] = ExecuteSingleHandlerAsync(
                            desc,
                            eventInstance,
                            scopeFactory,
                            serviceProvider,
                            preResolvedInstance,
                            linkedCts,
                            () =>
                            {
                                ILogger? loggerToUse;
                                lock (loggerSync)
                                {
                                    if (!loggerResolved)
                                    {
                                        multiLogger = serviceProvider.GetService(typeof(ILogger<ParallelExecutionStrategy>)) as ILogger;
                                        loggerResolved = true;
                                    }

                                    loggerToUse = multiLogger;
                                }

                                loggerToUse?.LogWarning(
                                    "Handler '{HandlerType}' registered for event '{EventType}' could not be resolved from service provider (GetService returned null). Skipping handler.",
                                    desc.HandlerType.FullName,
                                    typeof(TEvent).FullName);
                            },
                            effectiveToken,
                            throttler);
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
                            if (options.ErrorPolicy == ErrorHandlingPolicy.FailFast)
                            {
                                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
                            }

                            throw new EventDispatchException(typeof(TEvent), exceptions);
                        }

                        throw;
                    }
                    return;
                }
        }
    }

    private static async Task ExecuteSingleHandlerAsync<TEvent>(
        HandlerDescriptor descriptor,
        TEvent eventInstance,
        IServiceScopeFactory? scopeFactory,
        IServiceProvider fallbackProvider,
        object? preResolvedInstance,
        CancellationTokenSource? linkedCts,
        Action onUnresolved,
        CancellationToken cancellationToken,
        SemaphoreSlim? throttler = null)
        where TEvent : IEvent
    {
        if (Context.EventContext.IsHandlerCompleted(eventInstance.Id, descriptor.HandlerType))
        {
            return;
        }

        if (throttler != null)
        {
            await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            if (scopeFactory != null)
            {
                using var scope = scopeFactory.CreateScope();
                var instance = HandlerResolutionHelper.ResolveHandler<TEvent>(scope.ServiceProvider, descriptor);
                if (instance != null)
                {
                    var vt = descriptor.TypedInvoker is HandlerInvoker<TEvent> typedInvoker
                        ? typedInvoker(instance, eventInstance, cancellationToken)
                        : descriptor.Invoker(instance, eventInstance, cancellationToken);
                    if (!vt.IsCompletedSuccessfully)
                    {
                        await vt.ConfigureAwait(false);
                    }

                    Context.EventContext.MarkHandlerCompleted(eventInstance.Id, descriptor.HandlerType);
                }
                else
                {
                    onUnresolved();
                }
            }
            else
            {
                var instance = preResolvedInstance;
                if (instance != null)
                {
                    var vt = descriptor.TypedInvoker is HandlerInvoker<TEvent> typedInvoker
                        ? typedInvoker(instance, eventInstance, cancellationToken)
                        : descriptor.Invoker(instance, eventInstance, cancellationToken);
                    if (!vt.IsCompletedSuccessfully)
                    {
                        await vt.ConfigureAwait(false);
                    }

                    Context.EventContext.MarkHandlerCompleted(eventInstance.Id, descriptor.HandlerType);
                }
                else
                {
                    onUnresolved();
                }
            }
        }
        catch (Exception)
        {
            try { linkedCts?.Cancel(); } catch (ObjectDisposedException) { }
            throw;
        }
        finally
        {
            throttler?.Release();
        }
    }
}
