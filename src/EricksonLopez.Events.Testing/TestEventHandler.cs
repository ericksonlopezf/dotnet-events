// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Events.Contracts;

namespace EricksonLopez.Events.Testing;

/// <summary>
/// Provides a test spy and stub implementation of <see cref="IEventHandler{TEvent}"/> for verifying handler executions and simulating delays or failures.
/// </summary>
/// <typeparam name="TEvent">The event type to handle.</typeparam>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Implements IEventHandler for testing.")]
public sealed class TestEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    private readonly List<TEvent> _handledEvents = new();
    private readonly List<DateTimeOffset> _executionTimestamps = new();
    private readonly object _lock = new();

    private TimeSpan? _delay;
    private Exception? _exceptionToThrow;
    private Func<TEvent, CancellationToken, ValueTask>? _customCallback;

    /// <summary>
    /// Gets a read-only list of all events handled by this test handler in chronological order.
    /// </summary>
    public IReadOnlyList<TEvent> HandledEvents
    {
        get
        {
            lock (_lock)
            {
                return _handledEvents.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the timestamps when each event was handled.
    /// </summary>
    public IReadOnlyList<DateTimeOffset> ExecutionTimestamps
    {
        get
        {
            lock (_lock)
            {
                return _executionTimestamps.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the number of times this handler has been invoked.
    /// </summary>
    public int InvocationCount
    {
        get
        {
            lock (_lock)
            {
                return _handledEvents.Count;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this handler was invoked at least once.
    /// </summary>
    public bool WasInvoked => InvocationCount > 0;

    /// <summary>
    /// Gets the last event handled, or <see langword="null"/> if none handled.
    /// </summary>
    public TEvent? LastEvent
    {
        get
        {
            lock (_lock)
            {
                return _handledEvents.Count > 0 ? _handledEvents[^1] : default;
            }
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException"><paramref name="eventInstance"/> is <see langword="null"/></exception>
    public async ValueTask HandleAsync(TEvent eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        cancellationToken.ThrowIfCancellationRequested();

        if (_delay.HasValue)
        {
            await Task.Delay(_delay.Value, cancellationToken);
        }

        lock (_lock)
        {
            _handledEvents.Add(eventInstance);
            _executionTimestamps.Add(DateTimeOffset.UtcNow);

            if (_exceptionToThrow != null)
            {
                throw _exceptionToThrow;
            }
        }

        if (_customCallback != null)
        {
            await _customCallback(eventInstance, cancellationToken);
        }
    }

    /// <summary>
    /// Configures the handler to simulate an execution delay.
    /// </summary>
    /// <param name="delay">The delay duration.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is less than or equal to <see cref="TimeSpan.Zero"/></exception>
    public TestEventHandler<TEvent> WithDelay(TimeSpan delay)
    {
        if (delay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay duration must be greater than zero.");
        }

        _delay = delay;
        return this;
    }

    /// <summary>
    /// Configures the handler to throw the specified exception whenever invoked.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
    public TestEventHandler<TEvent> WithException(Exception exception)
    {
        _exceptionToThrow = exception ?? throw new ArgumentNullException(nameof(exception));
        return this;
    }

    /// <summary>
    /// Configures a synchronous callback delegate to execute on handle.
    /// </summary>
    /// <param name="callback">The callback delegate.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/></exception>
    public TestEventHandler<TEvent> WithCallback(Action<TEvent> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _customCallback = (evt, _) =>
        {
            callback(evt);
            return ValueTask.CompletedTask;
        };
        return this;
    }

    /// <summary>
    /// Configures an asynchronous callback delegate to execute on handle.
    /// </summary>
    /// <param name="asyncCallback">The async callback delegate.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="asyncCallback"/> is <see langword="null"/></exception>
    public TestEventHandler<TEvent> WithCallback(Func<TEvent, CancellationToken, ValueTask> asyncCallback)
    {
        _customCallback = asyncCallback ?? throw new ArgumentNullException(nameof(asyncCallback));
        return this;
    }

    /// <summary>
    /// Clears all recorded handled events, timestamps, and resets configurations.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _handledEvents.Clear();
            _executionTimestamps.Clear();
            _delay = null;
            _exceptionToThrow = null;
            _customCallback = null;
        }
    }
}
