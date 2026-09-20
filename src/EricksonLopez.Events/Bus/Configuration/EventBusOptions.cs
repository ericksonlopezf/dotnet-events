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

    /// <summary>
    /// Gets or sets the dependency injection scope resolution policy for event handlers.
    /// The default is <see cref="HandlerScopePolicy.Auto"/>.
    /// </summary>
    public HandlerScopePolicy ScopePolicy { get; set; } = HandlerScopePolicy.Auto;

    /// <summary>
    /// Gets or sets the maximum degree of parallelism when executing in parallel mode.
    /// The default is 0 (unbounded). A value greater than 0 limits concurrent handler executions.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; }

    /// <summary>
    /// Validates the event bus configuration options, ensuring concurrency invariants and valid depth limits.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when invalid options are configured, such as a non-positive <see cref="MaxReentrancyDepth"/>
    /// or a negative <see cref="MaxDegreeOfParallelism"/>.
    /// </exception>
    public void Validate()
    {
        if (MaxReentrancyDepth <= 0)
        {
            throw new InvalidOperationException($"{nameof(MaxReentrancyDepth)} must be greater than zero.");
        }

        if (MaxDegreeOfParallelism < 0)
        {
            throw new InvalidOperationException($"{nameof(MaxDegreeOfParallelism)} cannot be negative.");
        }

        if (ExecutionMode == EventExecutionMode.Parallel && ScopePolicy == HandlerScopePolicy.ReuseAmbientScope)
        {
            throw new InvalidOperationException(
                $"Configuring {nameof(ExecutionMode)} as '{nameof(EventExecutionMode.Parallel)}' with {nameof(ScopePolicy)} as '{nameof(HandlerScopePolicy.ReuseAmbientScope)}' is forbidden. " +
                "Concurrent handlers sharing a single scoped service provider risk corrupting non-thread-safe dependencies (such as DbContext). " +
                $"Use '{nameof(HandlerScopePolicy.CreatePerHandler)}' or '{nameof(HandlerScopePolicy.Auto)}' instead.");
        }
    }
}


