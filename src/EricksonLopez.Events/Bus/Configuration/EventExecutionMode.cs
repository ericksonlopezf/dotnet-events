// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.Bus.Configuration;

/// <summary>
/// Specifies how multiple handlers registered for the same event type are executed during dispatch.
/// </summary>
public enum EventExecutionMode
{
    /// <summary>
    /// Executes handlers sequentially in registration order.
    /// </summary>
    Sequential = 0,

    /// <summary>
    /// Executes handlers concurrently.
    /// </summary>
    Parallel = 1
}




