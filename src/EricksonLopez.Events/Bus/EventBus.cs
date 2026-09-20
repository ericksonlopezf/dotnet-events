// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Diagnostics;
using EricksonLopez.Events.Bus.Execution;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;
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
/// <para>
/// All event handlers must be registered explicitly during startup via
/// <see cref="EricksonLopez.Events.Bus.Extensions.EventBusServiceCollectionExtensions.AddEventHandler{TEvent,THandler}(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.DependencyInjection.ServiceLifetime)"/>
/// or the source-generated <c>AddGeneratedEventHandlers()</c> extension.
/// Dynamic discovery of handlers registered directly in DI as <c>IEventHandler&lt;TEvent&gt;</c> without explicit registration
/// is not supported: it caused race conditions under concurrency and is incompatible with NativeAOT.
/// </para>
/// </remarks>
public sealed class EventBus : IEventBus
{
    private readonly AsyncLocal<int> _reentrancyDepth = new();

    private readonly IHandlerRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventBusOptions _options;
    private readonly IExecutionStrategy _sequentialStrategy;
    private readonly IExecutionStrategy _parallelStrategy;
    private readonly IReadOnlyList<IEventMiddleware> _middlewares;
    private readonly ILogger<EventBus>? _logger;

    // Pipeline cache: per-event-type compiled middleware chain. Thread-safe: GetOrAdd is idempotent here
    // because the factory creates a pure function from immutable middleware list, so any race produces
    // equivalent delegates. This does NOT use the registry (no handler discovery race).
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, object> _pipelineCache = new();

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
        _options.Validate();
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

        int currentDepth = _reentrancyDepth.Value;
        if (currentDepth >= _options.MaxReentrancyDepth)
        {
            throw new InvalidOperationException(
                $"EventBus maximum reentrancy depth limit ({_options.MaxReentrancyDepth}) exceeded while publishing '{typeof(TEvent).Name}'. Possible cyclic event cascade.");
        }

        IDisposable? contextScope = null;
        bool isSameEvent = Context.EventContext.Current != null &&
            (ReferenceEquals(Context.EventContext.Current.GetPayload(), eventInstance) ||
             (Context.EventContext.Current.Id == eventInstance.Id &&
              Context.EventContext.Current.OccurredAt == eventInstance.OccurredAt));

        if (!isSameEvent)
        {
            var metadata = Context.EventContext.Current != null
                ? Metadata.EventMetadata.Create(
                    correlationId: Context.EventContext.CorrelationId,
                    causationId: Identifiers.CausationId.From(Context.EventContext.Current.Id),
                    tenantId: Context.EventContext.TenantId,
                    source: null,
                    customHeaders: Context.EventContext.Current.Metadata?.CustomHeaders)
                : Metadata.EventMetadata.Empty;

            var ephemeralEnvelope = Envelopes.EventEnvelope.Create(eventInstance, metadata);
            contextScope = Context.EventContext.SetCurrent(ephemeralEnvelope);
        }

        _reentrancyDepth.Value = currentDepth + 1;
        long startTimestamp = Stopwatch.GetTimestamp();
        using var activity = EventBusDiagnostics.StartPublishActivity(eventInstance);

        int handlerCount = 0;
        bool success = false;
        try
        {
            // EVT-HIGH-001 / EVT-HIGH-006 / EVT-HIGH-AOT-001 FIX:
            // Query the registry directly — no dynamic DI discovery at publish time.
            // All handlers must be pre-registered via AddEventHandler<TEvent, THandler>() or AddGeneratedEventHandlers().
            // The previous GetOrDiscoverHandlers() method had two critical defects:
            //   1. ConcurrentDictionary.GetOrAdd factory executed multiple times under high concurrency
            //      → duplicate HandlerDescriptors → handler invoked N times instead of once.
            //   2. typeof(IEnumerable<>).MakeGenericType(...) is incompatible with NativeAOT/trimming.
            var handlers = _registry.GetHandlers(typeof(TEvent));
            handlerCount = handlers.Count;
            if (handlers.Count == 0)
            {
                if (_options.ThrowOnUnregisteredEvent)
                {
                    throw new InvalidOperationException(
                        $"No handlers registered for event type '{typeof(TEvent).FullName}'. " +
                        $"Register a handler using services.AddEventHandler<{typeof(TEvent).Name}, THandler>() " +
                        "or services.AddGeneratedEventHandlers() (source generator). " +
                        "Set EventBusOptions.ThrowOnUnregisteredEvent = false to suppress this exception.");
                }

                EventBusDiagnostics.RecordPublish(typeof(TEvent).Name, 0, 0, true);
                return;
            }

            var strategy = _options.ExecutionMode == EventExecutionMode.Parallel
                ? _parallelStrategy
                : _sequentialStrategy;

            if (_middlewares.Count == 0)
            {
                var dispatchTask = strategy.ExecuteAsync(handlers, eventInstance, _serviceProvider, _options, cancellationToken);
                if (!dispatchTask.IsCompletedSuccessfully)
                {
                    await dispatchTask.ConfigureAwait(false);
                }
            }
            else
            {
                var pipeline = (EventMiddlewareDelegate<TEvent>)_pipelineCache.GetOrAdd(typeof(TEvent), _ =>
                {
                    // Factory builds a pure function from immutable state (registry + middleware list).
                    // Any concurrent race on GetOrAdd produces equivalent delegates — idempotent.
                    EventMiddlewareDelegate<TEvent> terminalDelegate = (evt, ct) =>
                    {
                        var currentHandlers = _registry.GetHandlers(typeof(TEvent));
                        return strategy.ExecuteAsync(currentHandlers, evt, _serviceProvider, _options, ct);
                    };

                    return MiddlewarePipeline.Build(_middlewares, terminalDelegate);
                });

                await pipeline(eventInstance, cancellationToken).ConfigureAwait(false);
            }

            success = true;
        }
        catch (Exception ex)
        {
            var errorMessage = ex is AggregateException agg
                ? string.Join("; ", agg.Flatten().InnerExceptions.Select(e => e.Message))
                : ex.Message;

            activity?.SetStatus(ActivityStatusCode.Error, errorMessage);
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
            {
                { "exception.message", errorMessage },
                { "exception.type", ex.GetType().FullName }
            }));
            _logger?.LogError(ex, "Error occurred while dispatching event '{EventType}': {ErrorMessage}", typeof(TEvent).Name, errorMessage);
            throw;
        }
        finally
        {
            contextScope?.Dispose();
            _reentrancyDepth.Value = currentDepth;
            double elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            EventBusDiagnostics.RecordPublish(typeof(TEvent).Name, elapsedMs, handlerCount, success);
        }
    }

    /// <inheritdoc />
    public async ValueTask PublishEnvelopeAsync<TEvent>(Envelopes.IEventEnvelope<TEvent> envelope, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(envelope);
        using (Context.EventContext.SetCurrent(envelope))
        {
            await PublishAsync(envelope.Payload, cancellationToken).ConfigureAwait(false);
        }
    }
}
