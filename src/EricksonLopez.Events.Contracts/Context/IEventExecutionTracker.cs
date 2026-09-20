// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Events.Identifiers;

namespace EricksonLopez.Events.Context;

/// <summary>
/// Defines a contract for tracking completed handler executions per event during dispatch,
/// enabling idempotent retries in transactional and resilient pipelines.
/// </summary>
public interface IEventExecutionTracker
{
    /// <summary>
    /// Determines whether the specified handler type has already completed processing for the given event ID.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="handlerType">The handler implementation or interface type.</param>
    /// <returns><see langword="true"/> if already completed; otherwise, <see langword="false"/>.</returns>
    bool IsCompleted(EventId eventId, Type handlerType);

    /// <summary>
    /// Records that the specified handler type has completed processing for the given event ID.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="handlerType">The handler implementation or interface type.</param>
    void MarkCompleted(EventId eventId, Type handlerType);
}
