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

namespace EricksonLopez.Events.UnitTests.Adversarial.Generators;

[Trait("Category", "Adversarial")]
public sealed class MultipleHandlersDiscoveredExecutionTests
{
    public sealed record MultiHandlerOrderPlaced(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class FirstOrderHandler : IEventHandler<MultiHandlerOrderPlaced>
    {
        public int ExecutionCount { get; private set; }

        public ValueTask HandleAsync(MultiHandlerOrderPlaced @event, CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class SecondOrderHandler : IEventHandler<MultiHandlerOrderPlaced>
    {
        public int ExecutionCount { get; private set; }

        public ValueTask HandleAsync(MultiHandlerOrderPlaced @event, CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task WhenMultipleHandlersRegisteredViaAddEventHandler_BothHandlersMustBeExecutedExactlyOnce()
    {
        // Verified Remediation for EVT-HIGH-001 — Post-Fix 1:
        // After removing late-binding DI discovery, handlers MUST use AddEventHandler().
        // This test verifies that registering multiple handlers for the same event via AddEventHandler()
        // results in each handler being executed exactly once — no duplicates, no misses.

        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ThrowOnUnregisteredEvent = false;
            opts.ExecutionMode = EventExecutionMode.Sequential;
        });

        // Use AddEventHandler instead of raw DI ServiceDescriptor — required post Fix 1.
        services.AddEventHandler<MultiHandlerOrderPlaced, FirstOrderHandler>(ServiceLifetime.Singleton);
        services.AddEventHandler<MultiHandlerOrderPlaced, SecondOrderHandler>(ServiceLifetime.Singleton);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new MultiHandlerOrderPlaced(EventId.New(), DateTimeOffset.UtcNow));

        var handler1 = sp.GetRequiredService<FirstOrderHandler>();
        var handler2 = sp.GetRequiredService<SecondOrderHandler>();

        handler1.ExecutionCount.Should().Be(1, "FirstOrderHandler must be executed exactly once.");
        handler2.ExecutionCount.Should().Be(1, "SecondOrderHandler must be executed exactly once.");
    }
}
