// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Exceptions;

/// <summary>
/// Represents errors that occur when an event type or payload fails validation rules.
/// </summary>
public sealed class EventValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EventValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventValidationException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public EventValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
