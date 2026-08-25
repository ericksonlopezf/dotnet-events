// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Events.UnitTests.Bus;

using EricksonLopez.Events.Bus.Configuration;
using EricksonLopez.Events.Bus.Extensions;
using EricksonLopez.Events.Bus.Middleware;
using EricksonLopez.Events.Bus.Registry;
using EricksonLopez.Events.Contracts;
using EricksonLopez.Events.Identifiers;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Xunit.Trait("Category", "Unit")]
public sealed class EventBusServiceCollectionExtensionsTests
{
    public sealed record SampleDiEvent(EventId Id, DateTimeOffset OccurredAt) : IEvent;

    public sealed class SampleDiHandler : IEventHandler<SampleDiEvent>
    {
        public int Invocations { get; private set; }

        public ValueTask HandleAsync(SampleDiEvent @event, CancellationToken cancellationToken = default)
        {
            Invocations++;
            return ValueTask.CompletedTask;
        }
    }

    public sealed class TestSpyMiddleware : IEventMiddleware
    {
        public bool Executed { get; private set; }

        public async ValueTask InvokeAsync<TEvent>(TEvent eventInstance, EventMiddlewareDelegate<TEvent> nextHandler, CancellationToken cancellationToken)
            where TEvent : IEvent
        {
            Executed = true;
            await nextHandler(eventInstance, cancellationToken);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ServiceCollectionExtensions_WithNullServices_ShouldThrowArgumentNullException(int nullArgIndex)
    {
        IServiceCollection services = null!;

        Action act = nullArgIndex switch
        {
            0 => () => services.AddEventBus(),
            1 => () => services.AddEventHandler<SampleDiEvent, SampleDiHandler>(),
            _ => () => services.AddEventMiddleware<TestSpyMiddleware>()
        };

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public async Task AddEventBus_WithCustomOptionsAndHandlers_ShouldResolveAndDispatchCorrectly()
    {
        var services = new ServiceCollection();

        services.AddEventBus(options =>
        {
            options.ExecutionMode = EventExecutionMode.Parallel;
            options.MaxReentrancyDepth = 15;
        });

        services.AddEventHandler<SampleDiEvent, SampleDiHandler>(ServiceLifetime.Singleton);
        services.AddEventMiddleware<TestSpyMiddleware>(ServiceLifetime.Singleton);

        using var sp = services.BuildServiceProvider();

        var options = sp.GetRequiredService<EventBusOptions>();
        options.ExecutionMode.Should().Be(EventExecutionMode.Parallel);
        options.MaxReentrancyDepth.Should().Be(15);

        var registry = sp.GetRequiredService<IHandlerRegistry>();
        registry.HasHandlers(typeof(SampleDiEvent)).Should().BeTrue();
        var handlers = registry.GetHandlers(typeof(SampleDiEvent));
        handlers.Should().HaveCount(1);
        handlers[0].HandlerType.Should().Be(typeof(SampleDiHandler));
        handlers[0].ServiceType.Should().Be(typeof(IEventHandler<SampleDiEvent>));

        var bus = sp.GetRequiredService<IEventBus>();
        var publisher = sp.GetRequiredService<IEventPublisher>();
        bus.Should().BeSameAs(publisher);

        var handler = sp.GetRequiredService<SampleDiHandler>();
        var interfaceHandler = sp.GetRequiredService<IEventHandler<SampleDiEvent>>();
        interfaceHandler.Should().BeSameAs(handler);

        var middleware = sp.GetRequiredService<IEventMiddleware>() as TestSpyMiddleware;

        var evt = new SampleDiEvent(EventId.New(), DateTimeOffset.UtcNow);
        await bus.PublishAsync(evt);

        handler.Invocations.Should().Be(1);
        middleware.Should().NotBeNull();
        middleware!.Executed.Should().BeTrue();

        // Verify invocation via the generated Invoker delegate in HandlerDescriptor
        await handlers[0].Invoker(handler, evt, CancellationToken.None);
        handler.Invocations.Should().Be(2);
    }

    [Fact]
    public void HandlerRegistrationToken_Constructor_ShouldSetProperties()
    {
        var desc = new HandlerDescriptor(typeof(SampleDiHandler), typeof(object), (_, _, _) => ValueTask.CompletedTask);
        var token = new HandlerRegistrationToken(typeof(SampleDiEvent), desc);

        token.EventType.Should().Be(typeof(SampleDiEvent));
        token.Descriptor.Should().BeSameAs(desc);
    }
}




