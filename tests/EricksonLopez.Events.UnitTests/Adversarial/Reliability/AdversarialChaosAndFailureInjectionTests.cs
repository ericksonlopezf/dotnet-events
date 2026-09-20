// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Exceptions;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Adversarial.Reliability;

[Trait("Category", "Chaos")]
public sealed class AdversarialChaosAndFailureInjectionTests
{
    public sealed record ChaosOrderPlaced(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class HealthyHandler1 : IEventHandler<ChaosOrderPlaced>
    {
        public bool Executed { get; private set; }
        public ValueTask HandleAsync(ChaosOrderPlaced eventInstance, CancellationToken cancellationToken = default)
        {
            Executed = true;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class PoisonHandler : IEventHandler<ChaosOrderPlaced>
    {
        public ValueTask HandleAsync(ChaosOrderPlaced eventInstance, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated catastrophic failure in poison handler.");
        }
    }

    public sealed class HealthyHandler2 : IEventHandler<ChaosOrderPlaced>
    {
        public bool Executed { get; private set; }
        public ValueTask HandleAsync(ChaosOrderPlaced eventInstance, CancellationToken cancellationToken = default)
        {
            Executed = true;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task SequentialMode_AggregateAndContinue_ExecutesRemainingHandlersDespitePoisonHandler()
    {
        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential;
            opts.ErrorPolicy = ErrorHandlingPolicy.AggregateAndContinue;
        });

        services.AddEventHandler<ChaosOrderPlaced, HealthyHandler1>(ServiceLifetime.Singleton);
        services.AddEventHandler<ChaosOrderPlaced, PoisonHandler>(ServiceLifetime.Singleton);
        services.AddEventHandler<ChaosOrderPlaced, HealthyHandler2>(ServiceLifetime.Singleton);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();
        var h1 = sp.GetRequiredService<HealthyHandler1>();
        var h2 = sp.GetRequiredService<HealthyHandler2>();

        Func<Task> act = async () => await bus.PublishAsync(new ChaosOrderPlaced(EventId.New(), DateTimeOffset.UtcNow));

        // When publishing, it should throw EventDispatchException aggregating the poison error
        var ex = await act.Should().ThrowAsync<EventDispatchException>();
        ex.Which.InnerExceptions.Should().HaveCount(1);
        ex.Which.EventType.Should().Be(typeof(ChaosOrderPlaced));

        // And healthy handlers both ran to completion!
        h1.Executed.Should().BeTrue();
        h2.Executed.Should().BeTrue();
    }

    [Fact]
    public async Task SequentialMode_FailFast_AbortsImmediatelyAndSkipsDownstreamHandlers()
    {
        var services = new ServiceCollection();
        services.AddEventBus(opts =>
        {
            opts.ExecutionMode = EventExecutionMode.Sequential;
            opts.ErrorPolicy = ErrorHandlingPolicy.FailFast;
        });

        services.AddEventHandler<ChaosOrderPlaced, HealthyHandler1>(ServiceLifetime.Singleton);
        services.AddEventHandler<ChaosOrderPlaced, PoisonHandler>(ServiceLifetime.Singleton);
        services.AddEventHandler<ChaosOrderPlaced, HealthyHandler2>(ServiceLifetime.Singleton);

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBus>();
        var h1 = sp.GetRequiredService<HealthyHandler1>();
        var h2 = sp.GetRequiredService<HealthyHandler2>();

        Func<Task> act = async () => await bus.PublishAsync(new ChaosOrderPlaced(EventId.New(), DateTimeOffset.UtcNow));

        // When publishing, it should throw the original InvalidOperationException
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Handler 1 executed before poison
        h1.Executed.Should().BeTrue();
        // Handler 2 was aborted and skipped!
        h2.Executed.Should().BeFalse();
    }
}
