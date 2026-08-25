// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using EricksonLopez.Events.UnitTests.Common;
using Xunit;

namespace EricksonLopez.Events.UnitTests.Bus;

[Trait("Category", "Integration")]
public sealed class EventBusIntegrationTests
{
    public sealed record SampleBusEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class FixtureTestHandler1 : IEventHandler<SampleBusEvent>
    {
        public bool Invoked { get; private set; }
        public ValueTask HandleAsync(SampleBusEvent @event, CancellationToken cancellationToken = default)
        {
            Invoked = true;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class FixtureTestHandler2 : IEventHandler<SampleBusEvent>
    {
        public bool Invoked { get; private set; }
        public ValueTask HandleAsync(SampleBusEvent @event, CancellationToken cancellationToken = default)
        {
            Invoked = true;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class FixtureLoggingMiddleware : IEventMiddleware
    {
        public int ExecutedCount { get; private set; }
        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> nextHandler, CancellationToken cancellationToken)
            where TEvent : IEvent
        {
            ExecutedCount++;
            await nextHandler(eventInstance, cancellationToken);
        }
    }

    [Fact]
    public async Task EventBus_WithTestFixture_ConfiguredHandlers_ShouldResolveAndDispatchSuccessfully()
    {
        using var fixture = new EventBusTestFixture()
            .WithOptions(opts => opts.ExecutionMode = EventExecutionMode.Sequential)
            .WithEventHandler<SampleBusEvent, FixtureTestHandler1>(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)
            .WithEventHandler<SampleBusEvent, FixtureTestHandler2>(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);

        var bus = fixture.GetBus();
        var h1 = fixture.GetService<FixtureTestHandler1>();
        var h2 = fixture.GetService<FixtureTestHandler2>();

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(evt);

        h1.Invoked.Should().BeTrue();
        h2.Invoked.Should().BeTrue();
    }

    [Fact]
    public async Task EventBus_WithTestFixture_WithOptionsAndMiddleware_ShouldExecutePipelineCorrectly()
    {
        using var fixture = new EventBusTestFixture()
            .WithOptions(opts => opts.ThrowOnUnregisteredEvent = false)
            .WithEventMiddleware<FixtureLoggingMiddleware>(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)
            .WithEventHandler<SampleBusEvent, FixtureTestHandler1>(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);

        var bus = fixture.GetBus();
        var h1 = fixture.GetService<FixtureTestHandler1>();
        var middleware = (FixtureLoggingMiddleware)fixture.GetService<IEnumerable<IEventMiddleware>>().Single();

        var evt = new SampleBusEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(evt);

        h1.Invoked.Should().BeTrue();
        middleware.ExecutedCount.Should().Be(1);
    }
}
