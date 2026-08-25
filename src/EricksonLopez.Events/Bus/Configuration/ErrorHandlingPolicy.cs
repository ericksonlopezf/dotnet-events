// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.Events.Bus.Configuration;

/// <summary>
/// Specifies how exceptions arising from individual event handlers are managed during dispatch.
/// </summary>
public enum ErrorHandlingPolicy
{
    /// <summary>
    /// Stops execution immediately upon encountering the first handler failure.
    /// </summary>
    FailFast = 0,

    /// <summary>
    /// Executes all handlers regardless of failures and aggregates all caught exceptions.
    /// </summary>
    AggregateAndContinue = 1
}
