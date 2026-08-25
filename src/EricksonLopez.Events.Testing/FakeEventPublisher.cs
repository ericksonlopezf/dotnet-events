// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.Events.Testing;

/// <summary>
/// Provides an in-memory fake event publisher for testing event emission, verifying assertions, and inspecting published events.
/// </summary>
public sealed class FakeEventPublisher : IEventPublisher
{
    private readonly List<IEvent> _publishedEvents = new();
    private readonly object _lock = new();
    private Exception? _simulatedException;
    private int _simulatedFailureCount;

    /// <summary>
    /// Gets a read-only list of all published events in chronological order.
    /// </summary>
    public IReadOnlyList<IEvent> PublishedEvents
    {
        get
        {
            lock (_lock)
            {
                return _publishedEvents.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the total number of events published.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _publishedEvents.Count;
            }
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="eventInstance"/> is <see langword="null"/></exception>
    public ValueTask PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            if (_simulatedFailureCount > 0)
            {
                _simulatedFailureCount--;
                throw _simulatedException!;
            }

            _publishedEvents.Add(eventInstance);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Simulates a failure on subsequent publish attempts by throwing the specified exception.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <param name="failureCount">The number of subsequent publishes that should fail. The default is 1.</param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="failureCount"/> is less than or equal to zero</exception>
    public void SimulateFailure(Exception exception, int failureCount = 1)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (failureCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(failureCount), "Failure count must be greater than zero.");
        }

        lock (_lock)
        {
            _simulatedException = exception;
            _simulatedFailureCount = failureCount;
        }
    }

    /// <summary>
    /// Retrieves all published events of the specified type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to filter by.</typeparam>
    /// <returns>A read-only list of matching events.</returns>
    public IReadOnlyList<TEvent> GetEvents<TEvent>() where TEvent : IEvent
    {
        lock (_lock)
        {
            var list = new List<TEvent>();
            foreach (var evt in _publishedEvents)
            {
                if (evt is TEvent match)
                {
                    list.Add(match);
                }
            }
            return list;
        }
    }

    /// <summary>
    /// Retrieves all published events of the specified type that match the predicate.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to filter by.</typeparam>
    /// <param name="predicate">The filter predicate to apply to published events.</param>
    /// <returns>A read-only list of matching events.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/></exception>
    public IReadOnlyList<TEvent> GetEvents<TEvent>(Func<TEvent, bool> predicate) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(predicate);
        lock (_lock)
        {
            var list = new List<TEvent>();
            foreach (var evt in _publishedEvents)
            {
                if (evt is TEvent match && predicate(match))
                {
                    list.Add(match);
                }
            }
            return list;
        }
    }

    /// <summary>
    /// Retrieves the single published event of the specified type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to retrieve.</typeparam>
    /// <returns>The single published event instance.</returns>
    /// <exception cref="InvalidOperationException">No event or more than one event of the specified type was published</exception>
    public TEvent GetSingleEvent<TEvent>() where TEvent : IEvent
    {
        lock (_lock)
        {
            var matches = GetEvents<TEvent>();
            if (matches.Count == 0)
            {
                throw new InvalidOperationException($"Expected exactly one event of type '{typeof(TEvent).Name}', but none were published.");
            }
            if (matches.Count > 1)
            {
                throw new InvalidOperationException($"Expected exactly one event of type '{typeof(TEvent).Name}', but {matches.Count} were published.");
            }
            return matches[0];
        }
    }

    /// <summary>
    /// Asserts that at least one event of the specified type was published.
    /// </summary>
    /// <typeparam name="TEvent">The expected event type.</typeparam>
    /// <returns>The current fake publisher instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">No event of the specified type was published</exception>
    public FakeEventPublisher ShouldHavePublished<TEvent>() where TEvent : IEvent
    {
        lock (_lock)
        {
            var matches = GetEvents<TEvent>();
            if (matches.Count == 0)
            {
                throw new InvalidOperationException($"Expected event of type '{typeof(TEvent).Name}' to have been published, but it was not.");
            }
            return this;
        }
    }

    /// <summary>
    /// Asserts that at least one event of the specified type matching the predicate was published.
    /// </summary>
    /// <typeparam name="TEvent">The expected event type.</typeparam>
    /// <param name="predicate">The predicate to match against.</param>
    /// <returns>The current fake publisher instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">No event of the specified type matching the predicate was published</exception>
    public FakeEventPublisher ShouldHavePublished<TEvent>(Func<TEvent, bool> predicate) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(predicate);
        lock (_lock)
        {
            bool found = false;
            foreach (var evt in _publishedEvents)
            {
                if (evt is TEvent match && predicate(match))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException($"Expected event of type '{typeof(TEvent).Name}' matching the predicate to have been published, but none matched.");
            }
            return this;
        }
    }

    /// <summary>
    /// Asserts that no events of the specified type were published.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <returns>The current fake publisher instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">One or more events of the specified type were published</exception>
    public FakeEventPublisher ShouldNotHavePublished<TEvent>() where TEvent : IEvent
    {
        lock (_lock)
        {
            var matches = GetEvents<TEvent>();
            if (matches.Count > 0)
            {
                throw new InvalidOperationException($"Expected no events of type '{typeof(TEvent).Name}' to have been published, but {matches.Count} were found.");
            }
            return this;
        }
    }

    /// <summary>
    /// Asserts that no events of the specified type matching the predicate were published.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="predicate">The predicate to check.</param>
    /// <returns>The current fake publisher instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">One or more events of the specified type matching the predicate were published</exception>
    public FakeEventPublisher ShouldNotHavePublished<TEvent>(Func<TEvent, bool> predicate) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(predicate);
        lock (_lock)
        {
            int count = 0;
            foreach (var evt in _publishedEvents)
            {
                if (evt is TEvent match && predicate(match))
                {
                    count++;
                }
            }

            if (count > 0)
            {
                throw new InvalidOperationException($"Expected no events of type '{typeof(TEvent).Name}' matching predicate to have been published, but {count} were found.");
            }
            return this;
        }
    }

    /// <summary>
    /// Asserts that exactly the specified number of events of the specified type were published.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="expectedCount">The expected number of published events.</param>
    /// <returns>The current fake publisher instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="expectedCount"/> is negative</exception>
    /// <exception cref="InvalidOperationException">The actual count of published events does not match <paramref name="expectedCount"/></exception>
    public FakeEventPublisher ShouldHavePublishedCount<TEvent>(int expectedCount) where TEvent : IEvent
    {
        if (expectedCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedCount), "Expected count cannot be negative.");
        }

        lock (_lock)
        {
            var actualCount = _publishedEvents.OfType<TEvent>().Count();
            if (actualCount != expectedCount)
            {
                throw new InvalidOperationException($"Expected {expectedCount} events of type '{typeof(TEvent).Name}' to have been published, but found {actualCount}.");
            }
            return this;
        }
    }

    /// <summary>
    /// Clears all recorded published events and resets simulated failure states.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _publishedEvents.Clear();
            _simulatedException = null;
            _simulatedFailureCount = 0;
        }
    }
}
