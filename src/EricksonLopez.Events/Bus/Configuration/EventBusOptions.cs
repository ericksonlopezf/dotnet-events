// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.Bus.Configuration;

/// <summary>
/// Provides configuration options for the in-process event bus.
/// </summary>
public sealed class EventBusOptions
{
    /// <summary>
    /// Gets or sets the execution mode for handlers dispatching the same event.
    /// The default is <see cref="EventExecutionMode.Sequential"/>.
    /// </summary>
    public EventExecutionMode ExecutionMode { get; set; } = EventExecutionMode.Sequential;

    /// <summary>
    /// Gets or sets the error handling policy applied when handler failures occur.
    /// The default is <see cref="ErrorHandlingPolicy.FailFast"/>.
    /// </summary>
    public ErrorHandlingPolicy ErrorPolicy { get; set; } = ErrorHandlingPolicy.FailFast;

    /// <summary>
    /// Gets or sets a value indicating whether an exception is thrown when publishing an event with no registered handlers.
    /// The default is <see langword="false"/>.
    /// </summary>
    public bool ThrowOnUnregisteredEvent { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowable reentrancy depth for nested event dispatch chains before throwing an exception.
    /// The default is 10.
    /// </summary>
    public int MaxReentrancyDepth { get; set; } = 10;
}


