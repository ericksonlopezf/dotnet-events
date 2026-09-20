// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Execution;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using EventId = EricksonLopez.Events.Identifiers.EventId;

namespace EricksonLopez.Events.UnitTests.Adversarial.Concurrency;

[Trait("Category", "Concurrency")]
public sealed class ParallelStrategyConcurrencyAndRaceTests
{
    public sealed record StressEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class SlowHandler : IEventHandler<StressEvent>
    {
        private static int s_activeConcurrency;
        private static int s_maxConcurrency;

        public static int ActiveConcurrency
        {
            get => Volatile.Read(ref s_activeConcurrency);
            set => Volatile.Write(ref s_activeConcurrency, value);
        }

        public static int MaxConcurrency
        {
            get => Volatile.Read(ref s_maxConcurrency);
            set => Volatile.Write(ref s_maxConcurrency, value);
        }

        public async ValueTask HandleAsync(StressEvent eventInstance, CancellationToken cancellationToken = default)
        {
            int active = Interlocked.Increment(ref s_activeConcurrency);
            lock (typeof(SlowHandler))
            {
                if (active > s_maxConcurrency) s_maxConcurrency = active;
            }
            await Task.Delay(10, cancellationToken);
            Interlocked.Decrement(ref s_activeConcurrency);
        }
    }

    [Fact]
    public async Task ParallelExecutionStrategy_UnderStress_ExecutesHandlersConcurrently()
    {
        SlowHandler.ActiveConcurrency = 0;
        SlowHandler.MaxConcurrency = 0;

        var strategy = new ParallelExecutionStrategy();
        var descriptors = new List<HandlerDescriptor>();

        for (int i = 0; i < 20; i++)
        {
            descriptors.Add(new HandlerDescriptor(
                typeof(SlowHandler),
                typeof(IEventHandler<StressEvent>),
                static (inst, evt, ct) => ((SlowHandler)inst).HandleAsync((StressEvent)evt, ct)));
        }

        var services = new ServiceCollection();
        services.AddTransient<SlowHandler>();
        var sp = services.BuildServiceProvider();

        var options = new EventBusOptions
        {
            ExecutionMode = EventExecutionMode.Parallel
        };

        var evt = new StressEvent(EventId.New(), DateTimeOffset.UtcNow);
        await strategy.ExecuteAsync(descriptors, evt, sp, options, CancellationToken.None);

        SlowHandler.MaxConcurrency.Should().BeGreaterThan(1,
            "ParallelExecutionStrategy should execute handlers concurrently across tasks.");
    }

    [Fact]
    public async Task ParallelExecutionStrategy_WithMultipleUnresolvedHandlers_DoesNotCrashFromLoggingRace()
    {
        // Tests concurrent execution when multiple handlers cannot be resolved from DI.
        // Verifies that the onUnresolved callback does not throw null reference or concurrency errors.
        var strategy = new ParallelExecutionStrategy();
        var descriptors = new List<HandlerDescriptor>();

        // Register 50 unregistered handler types to force concurrent onUnresolved calls:
        for (int i = 0; i < 50; i++)
        {
            descriptors.Add(new HandlerDescriptor(
                typeof(IEventHandler<StressEvent>),
                typeof(IEventHandler<StressEvent>),
                static (inst, evt, ct) => ValueTask.CompletedTask));
        }

        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var options = new EventBusOptions
        {
            ExecutionMode = EventExecutionMode.Parallel
        };

        var evt = new StressEvent(EventId.New(), DateTimeOffset.UtcNow);

        // Should complete without throwing NullReferenceException or race condition crash:
        Func<Task> act = async () => await strategy.ExecuteAsync(descriptors, evt, sp, options, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
