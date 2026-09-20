// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Dispatch;

using System.Collections.Concurrent;
using System.Diagnostics;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Diagnostics;

/// <summary>
/// Provides a lightweight in-memory event publisher and subscriber backed by a copy-on-write subscription list.
/// </summary>
/// <remarks>
/// <para>
/// Subscriptions and publications are thread-safe. The copy-on-write strategy ensures that iterating over handlers
/// during <c>PublishAsync</c> is not affected by concurrent calls to <see cref="Subscribe{TEvent}(IEventHandler{TEvent})"/> or <see cref="Unsubscribe{TEvent}(IEventHandler{TEvent})"/>.
/// </para>
/// <para>
/// Handlers are invoked sequentially in subscription order. Exceptions from individual handlers propagate immediately
/// and abort processing of remaining handlers for that publish call.
/// </para>
/// </remarks>
public sealed class InMemoryEventPublisher : IEventPublisher, IEventSubscriber
{
    private readonly ConcurrentDictionary<Type, CopyOnWriteList> _subscriptions = new();

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        var list = _subscriptions.GetOrAdd(typeof(TEvent), _ => new CopyOnWriteList());
        list.Add(handler);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (_subscriptions.TryGetValue(typeof(TEvent), out var list))
        {
            list.Remove(handler);
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    public void Subscribe<TEvent>(IEnvelopeEventHandler<TEvent> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        var list = _subscriptions.GetOrAdd(typeof(TEvent), _ => new CopyOnWriteList());
        list.Add(handler);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/></exception>
    public void Unsubscribe<TEvent>(IEnvelopeEventHandler<TEvent> handler) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (_subscriptions.TryGetValue(typeof(TEvent), out var list))
        {
            list.Remove(handler);
        }
    }

    private readonly AsyncLocal<int> _reentrancyDepth = new();

    /// <summary>
    /// Gets or sets the maximum allowable reentrancy depth before publication is aborted to prevent stack overflow.
    /// The default is 10.
    /// </summary>
    public int MaxReentrancyDepth { get; set; } = 10;

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="eventInstance"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The reentrancy depth limit was exceeded</exception>
    public async ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);

        int currentDepth = _reentrancyDepth.Value;
        if (currentDepth >= MaxReentrancyDepth)
        {
            throw new InvalidOperationException(
                $"InMemoryEventPublisher maximum reentrancy depth limit ({MaxReentrancyDepth}) exceeded while publishing '{typeof(TEvent).Name}'. Possible cyclic event cascade.");
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
        try
        {
            using var activity = EventsDiagnostics.StartPublishActivity(eventInstance);
            EventsDiagnostics.RecordEventPublished(typeof(TEvent).Name);

            if (!_subscriptions.TryGetValue(typeof(TEvent), out var list))
            {
                return;
            }

            var handlers = list.Items;
            for (int i = 0; i < handlers.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (handlers[i] is IEventHandler<TEvent> typedHandler)
                {
                    long startTimestamp = Stopwatch.GetTimestamp();
                    bool success = false;
                    try
                    {
                        await typedHandler.HandleAsync(eventInstance, cancellationToken).ConfigureAwait(false);
                        success = true;
                    }
                    finally
                    {
                        double elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
                        EventsDiagnostics.RecordEventHandled(typeof(TEvent).Name, elapsedMs, success);
                    }
                }
                else if (handlers[i] is IEnvelopeEventHandler<TEvent> envelopeHandler)
                {
                    var envelope = Context.EventContext.Current as Envelopes.IEventEnvelope<TEvent>
                        ?? Envelopes.EventEnvelope.Create(eventInstance);

                    long startTimestamp = Stopwatch.GetTimestamp();
                    bool success = false;
                    try
                    {
                        await envelopeHandler.HandleAsync(envelope, cancellationToken).ConfigureAwait(false);
                        success = true;
                    }
                    finally
                    {
                        double elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
                        EventsDiagnostics.RecordEventHandled(typeof(TEvent).Name, elapsedMs, success);
                    }
                }
            }
        }
        finally
        {
            contextScope?.Dispose();
            _reentrancyDepth.Value = currentDepth;
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

    private sealed class CopyOnWriteList
    {
        private readonly object _lock = new();
        private volatile object[] _items = Array.Empty<object>();

        public object[] Items => _items;

        public void Add(object item)
        {
            lock (_lock)
            {
                if (Array.IndexOf(_items, item) >= 0)
                {
                    return;
                }

                var newArray = new object[_items.Length + 1];
                Array.Copy(_items, newArray, _items.Length);
                newArray[^1] = item;
                _items = newArray;
            }
        }

        public void Remove(object item)
        {
            lock (_lock)
            {
                int index = Array.IndexOf(_items, item);
                if (index < 0) return;

                if (_items.Length == 1)
                {
                    _items = Array.Empty<object>();
                    return;
                }

                var newArray = new object[_items.Length - 1];
                Array.Copy(_items, 0, newArray, 0, index);
                Array.Copy(_items, index + 1, newArray, index, _items.Length - index - 1);
                _items = newArray;
            }
        }
    }
}




