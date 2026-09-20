// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using EricksonLopez.Events.Envelopes;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.Metadata;

namespace EricksonLopez.Events.Context;

/// <summary>
/// Provides ambient context for the event envelope currently being processed within the asynchronous execution flow.
/// </summary>
public static class EventContext
{
    private static readonly AsyncLocal<IEventEnvelope?> s_current = new();

    private static readonly AsyncLocal<IEventExecutionTracker?> s_tracker = new();

    /// <summary>
    /// Gets the envelope of the event currently being processed, or <see langword="null"/> if no event envelope is active.
    /// </summary>
    public static IEventEnvelope? Current => s_current.Value;

    /// <summary>
    /// Gets the ambient event metadata of the event currently being processed, or <see langword="null"/>.
    /// </summary>
    public static EventMetadata? Metadata => s_current.Value?.Metadata;

    /// <summary>
    /// Gets the tenant identifier of the current event, or <see langword="null"/>.
    /// </summary>
    public static TenantId? TenantId => s_current.Value?.Metadata.TenantId;

    /// <summary>
    /// Gets the correlation identifier of the current event, or <see langword="null"/>.
    /// </summary>
    public static CorrelationId? CorrelationId => s_current.Value?.Metadata.CorrelationId;

    /// <summary>
    /// Gets the causation identifier of the current event, or <see langword="null"/>.
    /// </summary>
    public static CausationId? CausationId => s_current.Value?.Metadata.CausationId;

    /// <summary>
    /// Gets the ambient event execution tracker, or <see langword="null"/> if no tracker is active.
    /// </summary>
    public static IEventExecutionTracker? ExecutionTracker => s_tracker.Value;

    /// <summary>
    /// Determines whether the specified handler has already completed processing for the given event ID.
    /// </summary>
    public static bool IsHandlerCompleted(EventId eventId, Type handlerType) =>
        s_tracker.Value?.IsCompleted(eventId, handlerType) ?? false;

    /// <summary>
    /// Marks the specified handler as completed processing for the given event ID.
    /// </summary>
    public static void MarkHandlerCompleted(EventId eventId, Type handlerType) =>
        s_tracker.Value?.MarkCompleted(eventId, handlerType);

    /// <summary>
    /// Sets the current event execution tracker within an ambient asynchronous scope.
    /// </summary>
    /// <param name="tracker">The event execution tracker to attach.</param>
    /// <returns>A disposable scope that restores the previous tracker upon disposal.</returns>
    public static IDisposable SetExecutionTracker(IEventExecutionTracker? tracker)
    {
        var previous = s_tracker.Value;
        s_tracker.Value = tracker;
        return new TrackerScope(previous);
    }

    /// <summary>
    /// Sets the current event envelope within an ambient asynchronous scope.
    /// </summary>
    /// <param name="envelope">The event envelope to attach to the ambient context.</param>
    /// <returns>A disposable scope that restores the previous ambient envelope upon disposal.</returns>
    public static IDisposable SetCurrent(IEventEnvelope? envelope)
    {
        var previous = s_current.Value;
        s_current.Value = envelope;
        return new Scope(previous);
    }

    private sealed class Scope(IEventEnvelope? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                s_current.Value = previous;
                _disposed = true;
            }
        }
    }

    private sealed class TrackerScope(IEventExecutionTracker? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                s_tracker.Value = previous;
                _disposed = true;
            }
        }
    }
}
