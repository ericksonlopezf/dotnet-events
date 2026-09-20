// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Concurrency;

[Trait("Category", "Adversarial")]
public sealed class ParallelSharedScopeConcurrencyTests
{
    public sealed record ParallelScopedEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    // Simulates a non-thread-safe scoped service like DbContext or UnitOfWork
    public sealed class NonThreadSafeScopedSession
    {
        private int _inFlightOperations;

        public async Task ExecuteOperationAsync(int delayMs)
        {
            if (Interlocked.Increment(ref _inFlightOperations) > 1)
            {
                throw new InvalidOperationException("Concurrency collision: NonThreadSafeScopedSession was accessed concurrently by multiple handlers!");
            }

            try
            {
                await Task.Delay(delayMs);
            }
            finally
            {
                Interlocked.Decrement(ref _inFlightOperations);
            }
        }
    }

    public sealed class HandlerA : IEventHandler<ParallelScopedEvent>
    {
        private readonly NonThreadSafeScopedSession _session;
        public HandlerA(NonThreadSafeScopedSession session) => _session = session;

        public async ValueTask HandleAsync(ParallelScopedEvent eventInstance, CancellationToken cancellationToken = default) =>
            await _session.ExecuteOperationAsync(50);
    }

    public sealed class HandlerB : IEventHandler<ParallelScopedEvent>
    {
        private readonly NonThreadSafeScopedSession _session;
        public HandlerB(NonThreadSafeScopedSession session) => _session = session;

        public async ValueTask HandleAsync(ParallelScopedEvent eventInstance, CancellationToken cancellationToken = default) =>
            await _session.ExecuteOperationAsync(50);
    }

    [Fact]
    public void EVT_CONC_004_ParallelExecution_WithReuseAmbientScope_IsProhibitedByValidation()
    {
        // Verified Remediation for EVT-CONC-004:
        // When HandlerScopePolicy.ReuseAmbientScope is configured with EventExecutionMode.Parallel,
        // EventBusOptions.Validate() throws InvalidOperationException at startup to prevent concurrent DbContext corruption.

        var options = new EventBusOptions
        {
            ExecutionMode = EventExecutionMode.Parallel,
            ScopePolicy = HandlerScopePolicy.ReuseAmbientScope
        };

        Action act = () => options.Validate();

        var ex = act.Should().Throw<InvalidOperationException>();
        ex.WithMessage("*Configuring ExecutionMode as 'Parallel' with ScopePolicy as 'ReuseAmbientScope' is forbidden*");
    }
}
