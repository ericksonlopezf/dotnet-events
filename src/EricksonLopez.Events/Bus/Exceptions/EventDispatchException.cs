// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Events.Bus.Exceptions;


/// <summary>
/// Represents errors that occur when one or more event handlers fail during event dispatch.
/// </summary>
public sealed class EventDispatchException : AggregateException
{
    /// <summary>
    /// Gets the type of the event that failed to be dispatched.
    /// </summary>
    public Type EventType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDispatchException"/> class for the specified event type and handler exceptions.
    /// </summary>
    /// <param name="eventType">The type of the event that failed dispatch.</param>
    /// <param name="innerExceptions">The collection of exceptions thrown by individual handlers.</param>
    public EventDispatchException(Type eventType, IEnumerable<Exception> innerExceptions)
        : base($"One or more handlers failed while dispatching event '{eventType.Name}'.", innerExceptions)
    {
        EventType = eventType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDispatchException"/> class with a specified error message.
    /// </summary>
    /// <param name="eventType">The type of the event that failed dispatch.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerExceptions">The collection of exceptions thrown by individual handlers.</param>
    public EventDispatchException(Type eventType, string message, IEnumerable<Exception> innerExceptions)
        : base(message, innerExceptions)
    {
        EventType = eventType;
    }
}


