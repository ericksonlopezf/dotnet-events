// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Concurrency;

[Trait("Category", "Concurrency")]
public sealed class Stress10kConcurrentPublishersTests
{
    public sealed record StressEvent(EventId Id, DateTimeOffset OccurredAt, int Index) : IEvent;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Test consumer")]
    public sealed class StressEventConsumer : IEventHandler<StressEvent>
    {
        private static int s_processedCount;
        public static int ProcessedCount => s_processedCount;

        public ValueTask HandleAsync(StressEvent @event, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref s_processedCount);
            return ValueTask.CompletedTask;
        }

        public static void Reset() => s_processedCount = 0;
    }

    [Fact]
    public async Task EventBus_Under10kConcurrentPublishers_DispatchesAllEventsWithoutLossOrRace()
    {
        StressEventConsumer.Reset();

        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential;
            opts.ThrowOnUnregisteredEvent = true;
        });
        services.AddEventHandler<StressEvent, StressEventConsumer>();

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        const int totalEvents = 10_000;
        var tasks = new Task[totalEvents];

        for (int i = 0; i < totalEvents; i++)
        {
            int index = i;
            tasks[i] = Task.Run(async () =>
            {
                var evt = new StressEvent(EventId.New(), DateTimeOffset.UtcNow, index);
                await bus.PublishAsync(evt);
            });
        }

        await Task.WhenAll(tasks);

        StressEventConsumer.ProcessedCount.Should().Be(totalEvents,
            "10,000 concurrent event publications must all be received and processed with zero event loss or race conditions.");
    }
}
