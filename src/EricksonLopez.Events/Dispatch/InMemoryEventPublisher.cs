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
/// during <see cref="PublishAsync{TEvent}"/> is not affected by concurrent calls to <see cref="Subscribe{TEvent}"/> or <see cref="Unsubscribe{TEvent}"/>.
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
    /// <exception cref="ArgumentNullException"><paramref name="eventInstance"/> is <see langword="null"/></exception>
    public async ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);

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




