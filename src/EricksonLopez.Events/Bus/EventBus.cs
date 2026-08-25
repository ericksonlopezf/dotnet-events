// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Diagnostics;
using EricksonLopez.Events.Bus.Execution;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides an in-process event bus for dispatching events to registered handlers through a configurable middleware pipeline.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EventBus"/> enforces a configurable reentrancy depth limit (<see cref="EventBusOptions.MaxReentrancyDepth"/>)
/// using <see cref="System.Threading.AsyncLocal{T}"/> to prevent cyclic event cascades across asynchronous execution contexts.
/// </para>
/// <para>
/// Middleware registered via <see cref="IEventMiddleware"/> is applied in the order returned by the service collection
/// and wraps all handler dispatch, including both <see cref="EventExecutionMode.Sequential"/> and <see cref="EventExecutionMode.Parallel"/> strategies.
/// </para>
/// </remarks>
public sealed class EventBus : IEventBus
{
    private static readonly AsyncLocal<int> ReentrancyDepth = new();

    private readonly IHandlerRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventBusOptions _options;
    private readonly IExecutionStrategy _sequentialStrategy;
    private readonly IExecutionStrategy _parallelStrategy;
    private readonly IReadOnlyList<IEventMiddleware> _middlewares;
    private readonly ILogger<EventBus>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBus"/> class with the specified dependencies and options.
    /// </summary>
    /// <param name="registry">The handler registry containing event-to-handler mappings.</param>
    /// <param name="serviceProvider">The service provider for resolving scoped handler instances.</param>
    /// <param name="options">The event bus configuration options.</param>
    /// <param name="middlewares">The optional collection of middlewares to execute around event dispatching.</param>
    /// <param name="logger">The optional logger instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="registry"/>, <paramref name="serviceProvider"/>, or <paramref name="options"/> is <see langword="null"/></exception>
    public EventBus(
        IHandlerRegistry registry,
        IServiceProvider serviceProvider,
        EventBusOptions options,
        IEnumerable<IEventMiddleware>? middlewares = null,
        ILogger<EventBus>? logger = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _sequentialStrategy = new SequentialExecutionStrategy();
        _parallelStrategy = new ParallelExecutionStrategy();
        _middlewares = middlewares != null ? new List<IEventMiddleware>(middlewares) : Array.Empty<IEventMiddleware>();
        _logger = logger;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="eventInstance"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The reentrancy depth limit was exceeded, or no handlers were registered and <see cref="EventBusOptions.ThrowOnUnregisteredEvent"/> is enabled</exception>
    [SuppressMessage("Usage", "CA1848:Use the LoggerMessage delegates", Justification = "Fallback exception logging path")]
    public async ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        cancellationToken.ThrowIfCancellationRequested();

        int currentDepth = ReentrancyDepth.Value;
        if (currentDepth >= _options.MaxReentrancyDepth)
        {
            throw new InvalidOperationException(
                $"EventBus maximum reentrancy depth limit ({_options.MaxReentrancyDepth}) exceeded while publishing '{typeof(TEvent).Name}'. Possible cyclic event cascade.");
        }

        ReentrancyDepth.Value = currentDepth + 1;
        long startTimestamp = Stopwatch.GetTimestamp();
        using var activity = EventBusDiagnostics.StartPublishActivity(eventInstance);

        var handlers = _registry.GetHandlers(typeof(TEvent));
        if (handlers.Count == 0)
        {
            if (_options.ThrowOnUnregisteredEvent)
            {
                throw new InvalidOperationException($"No handlers registered for event '{typeof(TEvent).FullName}'.");
            }

            ReentrancyDepth.Value = currentDepth;
            EventBusDiagnostics.RecordPublish(typeof(TEvent).Name, 0, 0, true);
            return;
        }

        bool success = false;
        try
        {
            var strategy = _options.ExecutionMode == EventExecutionMode.Parallel
                ? _parallelStrategy
                : _sequentialStrategy;

            EventMiddlewareDelegate<TEvent> terminalDelegate = (evt, ct) =>
                strategy.ExecuteAsync(handlers, evt, _serviceProvider, _options, ct);

            var pipeline = MiddlewarePipeline.Build(_middlewares, terminalDelegate);
            await pipeline(eventInstance, cancellationToken).ConfigureAwait(false);

            success = true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
            {
                { "exception.message", ex.Message },
                { "exception.type", ex.GetType().FullName }
            }));
            _logger?.LogError(ex, "Error occurred while dispatching event '{EventType}'", typeof(TEvent).Name);
            throw;
        }
        finally
        {
            ReentrancyDepth.Value = currentDepth;
            double elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            EventBusDiagnostics.RecordPublish(typeof(TEvent).Name, elapsedMs, handlers.Count, success);
        }
    }
}



