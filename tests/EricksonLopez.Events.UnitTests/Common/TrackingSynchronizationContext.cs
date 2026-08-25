// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Events.UnitTests.Common;

using System.Threading;

/// <summary>
/// A synchronization context that tracks post invocations to verify that
/// asynchronous continuations properly use ConfigureAwait(false) and do not post back to the capture context.
/// </summary>
public sealed class TrackingSynchronizationContext : SynchronizationContext
{
    public int PostCount { get; private set; }

    public override void Post(SendOrPostCallback d, object? state)
    {
        PostCount++;
        d(state);
    }
}

